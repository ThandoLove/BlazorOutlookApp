using Microsoft.Extensions.Caching.Memory;
using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.Exceptions;
using OperationalWorkspaceApplication.IRepositories;
using OperationalWorkspaceApplication.IServices;
using OperationalWorkspaceApplication.Jobs;
using OperationalWorkspaceApplication.Mappers;

using OperationalWorkspaceApplication.Requests.CustomerRequest;
using OperationalWorkspaceApplication.Responses.WorkspaceContextResponse;
using OperationalWorkspaceApplication.Services;
using OperationalWorkspaceDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Handlers;

public class GetCustomerContextQueryHandler
{
    private readonly ISageX3GraphQLClient _graphQlClient;
    private readonly IPremiumWorkspaceEngine _premiumEngine;
    private readonly IAuditLogQueue _auditQueue;
    private readonly ISageX3Configuration _config;
    private readonly IDocumentRepository _documentRepository;
    private readonly IMemoryCache _memoryCache; // Phase 8 Audit: Central Caching Layer

    private static readonly SemaphoreSlim _cacheLock = new(1, 1);
    private const string ErpCachePrefix = "ERP_AGGREGATED_CONTEXT_";

    public GetCustomerContextQueryHandler(
        ISageX3GraphQLClient graphQlClient,
        IPremiumWorkspaceEngine premiumEngine,
        IAuditLogQueue auditQueue,
        ISageX3Configuration config,
        IDocumentRepository documentRepository,
        IMemoryCache memoryCache)
    {
        _graphQlClient = graphQlClient;
        _premiumEngine = premiumEngine;
        _auditQueue = auditQueue;
        _config = config;
        _documentRepository = documentRepository;
        _memoryCache = memoryCache;
    }

    public async Task<WorkspaceContextResponse> HandleAsync(GetCustomerContextQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // 1. Business Filter Rule: Short-circuit internal company addresses immediately
        if (query.SenderEmail.EndsWith("@yourcompany.com", StringComparison.OrdinalIgnoreCase))
        {
            return new WorkspaceContextResponse(
                null, new List<SageDocumentDto>(),
                "• SENDER TYPE: Internal Employee Group Account.\n• CONTEXT METRICS: Standard compact task pane limits apply.",
                true, "Route operational queries directly to workspace task backlogs.", "Success"
            );
        }

        string cleanEmail = query.SenderEmail.Trim().ToLowerInvariant();
        string cacheKey = $"{ErpCachePrefix}{cleanEmail}";

        // 2. Optimization: Check Memory Cache first to prevent redundant lookups (Section 6)
        if (_memoryCache.TryGetValue(cacheKey, out WorkspaceContextResponse? cachedContext) && cachedContext != null)
        {
            return cachedContext;
        }

        // Thread-safe lock protects backend from concurrent request spikes for the same customer
        await _cacheLock.WaitAsync(ct);
        try
        {
            if (_memoryCache.TryGetValue(cacheKey, out WorkspaceContextResponse? doubleCheckContext) && doubleCheckContext != null)
            {
                return doubleCheckContext;
            }

            Customer? customerEntity;
            List<SageDocument> documentEntities = new();

            if (_config.UseMocks)
            {
                if (cleanEmail.Contains("acme") || cleanEmail.Contains("customer"))
                {
                    customerEntity = Customer.CreateHydratedRecord("BPC-ACME001", "Acme Global Manufacturing Ltd", query.SenderName, query.SenderEmail, "Active", 50000.00m, 12450.00m, "SR-909");
                    documentEntities.Add(SageDocument.MapExternalRecord("INV-2026-004", "Invoice", DateTime.UtcNow.AddDays(-2), 8450.00m, "USD", "Unpaid", "BLOB-811"));
                    documentEntities.Add(SageDocument.MapExternalRecord("INV-2026-001", "Invoice", DateTime.UtcNow.AddDays(-30), 4000.00m, "USD", "Overdue", "BLOB-204"));
                }
                else
                {
                    throw new CustomerNotFoundException(query.SenderEmail);
                }
            }
            else
            {
                // Live Integration Path aggregated using a single network request boundary
                customerEntity = await _graphQlClient.FetchUnifiedCustomerContextAsync(query.SenderEmail, ct);
                if (customerEntity != null)
                {
                    var rawDocs = await _documentRepository.GetByCustomerCodeAsync(customerEntity.Id, ct);

                    // Section 7 Resolution: Enforce Pagination and Capping limits natively (Max 50 recent items)
                    documentEntities = rawDocs
                        .OrderByDescending(d => d.Date)
                        .Take(50)
                        .ToList();
                }
            }

            if (customerEntity == null)
            {
                return new WorkspaceContextResponse(null, new List<SageDocumentDto>(), "No matching Sage X3 accounts matched.", false, "Secondary entry paths initialized.", "Secondary");
            }

            // Record telemetry data asynchronously
            var audit = AuditLog.GenerateHistoricalAuditEntry(query.ActiveUserEmail, "VIEW_CUSTOMER_PROFILE", customerEntity.Id, query.NetworkIp, string.Empty);
            _auditQueue.QueueAuditEntry(audit);

            var customerDto = IdentityModelMapper.MapToDto(customerEntity);
            var docDtos = documentEntities.Select(IdentityModelMapper.MapToDto).ToList();

            string cardText = "✔ Credit lines standing clear inside safe operating metrics.";
            string marker = "Success";

            if (customerEntity.IsHighRiskExposure)
            {
                cardText = "⚠️ High Credit Exposure Risk: Balance has climbed beyond 90% of allowance metrics!";
                marker = "Danger";
            }

            string aiSummaryBrief = _premiumEngine.CompileLocalScenarioBriefText(customerDto, docDtos);

            var aggregatedResponse = new WorkspaceContextResponse(customerDto, docDtos, aiSummaryBrief, false, cardText, marker);

            // 3. Cache the final aggregated response with a 3-minute sliding window (Section 6)
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            _memoryCache.Set(cacheKey, aggregatedResponse, cacheOptions);

            return aggregatedResponse;
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}
