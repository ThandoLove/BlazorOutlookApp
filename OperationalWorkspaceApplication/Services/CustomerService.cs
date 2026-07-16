using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.IServices;
using OperationalWorkspaceApplication.Jobs;
using OperationalWorkspaceApplication.Mappers;
using OperationalWorkspaceApplication.Requests.CustomerRequest;
using OperationalWorkspaceApplication.Responses.WorkspaceContextResponse;
using OperationalWorkspaceDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Services;

/// <summary>
/// Architectural Contract Interface to isolate Configuration details from the Application Layer.
/// Define this inside your application space (e.g. creating an ISageX3Configuration.cs file if not yet existing).
/// </summary>


public class CustomerService : ICustomerService
{
    private readonly ISageX3GraphQLClient _graphQlClient;
    private readonly IPremiumWorkspaceEngine _premiumEngine;
    private readonly IAuditLogQueue _auditQueue;
    private readonly ISageX3Configuration _config; // Injected pure contract abstraction interface

    public CustomerService(
        ISageX3GraphQLClient graphQlClient,
        IPremiumWorkspaceEngine premiumEngine,
        IAuditLogQueue auditQueue,
        ISageX3Configuration config) // Structural Dependency Inversion Fix
    {
        _graphQlClient = graphQlClient;
        _premiumEngine = premiumEngine;
        _auditQueue = auditQueue;
        _config = config;
    }

    public async Task<WorkspaceContextResponse> ResolveWorkspaceContextAsync(GetCustomerContextQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Rule: Verify corporate internal system structures natively
        if (query.SenderEmail.EndsWith("@yourcompany.com", StringComparison.OrdinalIgnoreCase))
        {
            return new WorkspaceContextResponse(
                null, new List<SageDocumentDto>(),
                "• SENDER TYPE: Internal Employee Group Account.\n• CONTEXT METRICS: Standard task pane departmental routing bounds apply.",
                true, "Route operational queries directly to workspace task backlogs.", "Success"
            );
        }

        Customer? customerEntity;
        List<SageDocument> documentEntities = new();

        // Evaluates properties directly off your clean Application boundary contract interface
        if (_config.UseMocks)
        {
            // Execute simulated isolated operational contexts instantly bypassing live endpoints
            if (query.SenderEmail.Contains("acme") || query.SenderEmail.Contains("customer"))
            {
                customerEntity = Customer.CreateHydratedRecord("BPC-ACME001", "Acme Global Manufacturing Ltd", query.SenderName, query.SenderEmail, "Active", 50000.00m, 12450.00m, "SR-909");
                documentEntities.Add(SageDocument.MapExternalRecord("INV-2026-004", "Invoice", DateTime.UtcNow.AddDays(-2), 8450.00m, "USD", "Unpaid", "BLOB-811"));
                documentEntities.Add(SageDocument.MapExternalRecord("INV-2026-001", "Invoice", DateTime.UtcNow.AddDays(-30), 4000.00m, "USD", "Overdue", "BLOB-204"));
            }
            else
            {
                // Cold Lead Scenario mappings
                var coldBrief = "• CONTEXT OVERVIEW: Unrecognized sender domain parameters.\n• ONBOARDING RECOMMENDATION: Intercept inquiries via sales CRM routing modules.";
                return new WorkspaceContextResponse(null, new List<SageDocumentDto>(), coldBrief, false, "💡 High Onboarding Conversion Potential. Forward to CRM pipeline.", "Warning");
            }
        }
        else
        {
            customerEntity = await _graphQlClient.FetchUnifiedCustomerContextAsync(query.SenderEmail, ct);
            if (customerEntity != null)
            {
                documentEntities = await _graphQlClient.FetchCustomerOrdersAndInvoicesAsync(customerEntity.Id, ct);
            }
        }

        if (customerEntity == null)
        {
            return new WorkspaceContextResponse(null, new List<SageDocumentDto>(), "No registered matching Sage X3 accounts matched.", false, "Create raw lead card entry tracking bounds.", "Secondary");
        }

        // Enqueue high-performance audit tracking data elements asynchronously via thread channel
        var audit = AuditLog.GenerateHistoricalAuditEntry(query.ActiveUserEmail, "VIEW_CUSTOMER_PROFILE", customerEntity.Id, query.NetworkIp, string.Empty);
        _auditQueue.QueueAuditEntry(audit);

        // Projections
        var customerDto = IdentityModelMapper.MapToDto(customerEntity);
        var docDtos = documentEntities.Select(IdentityModelMapper.MapToDto).ToList();

        // Calculate custom predictive alert indicators
        string cardText = "✔ Credit lines standing clear inside safe operating metrics.";
        string marker = "Success";

        if (customerEntity.IsHighRiskExposure)
        {
            cardText = "⚠️ High Credit Exposure Risk: Balance has climbed beyond 90% of allowance metrics!";
            marker = "Danger";
        }

        // Build premium optimization summaries
        string aiSummaryBrief = _premiumEngine.CompileLocalScenarioBriefText(customerDto, docDtos);

        return new WorkspaceContextResponse(customerDto, docDtos, aiSummaryBrief, false, cardText, marker);
    }
}
