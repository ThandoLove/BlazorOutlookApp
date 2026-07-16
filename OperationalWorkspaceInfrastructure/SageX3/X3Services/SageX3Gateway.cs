
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OperationalWorkspace.Infrastructure.SageX3.X3Dtos;
using OperationalWorkspaceApplication.IServices;
using OperationalWorkspaceApplication.Services;
using OperationalWorkspaceDomain.Entities;

using OperationalWorkspaceInfrastructure.SageX3.X3Mappers;

namespace OperationalWorkspaceInfrastructure.SageX3.X3Services;

public interface ISageX3Gateway
{
    Task<Customer?> FetchCustomerContextAsync(string email, CancellationToken ct);
    Task<List<SageDocument>> FetchRecentDocumentsAsync(string bpCode, CancellationToken ct);
}

public class SageX3Gateway : ISageX3Gateway
{
    private readonly HttpClient _httpClient;
    private readonly ISageAuthService _authService;
    private readonly ILogger<SageX3Gateway> _logger;

    public SageX3Gateway(HttpClient httpClient, ISageAuthService authService, ILogger<SageX3Gateway> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _logger = logger;
    }

    public async Task<Customer?> FetchCustomerContextAsync(string email, CancellationToken ct)
    {
        try
        {
            await InjectBearerTokenHeaderAsync(ct);

            // Clean abstraction: Query the gateway endpoint inside a secure thread boundary
            var response = await _httpClient.GetAsync($"api1/v1/businesspartners?email={Uri.EscapeDataString(email)}", ct);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var rawDto = await response.Content.ReadFromJsonAsync<SageX3BusinessPartnerDto>(cancellationToken: ct);
            if (rawDto == null) return null;

            // Insulate layers: Convert raw data to a clean internal entity using the SageMapper
            return SageMapper.MapToDomain(rawDto, email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sage X3 Gateway failure while resolving business partner context for {Email}", email);
            throw;
        }
    }

    public async Task<List<SageDocument>> FetchRecentDocumentsAsync(string bpCode, CancellationToken ct)
    {
        try
        {
            await InjectBearerTokenHeaderAsync(ct);

            var response = await _httpClient.GetAsync($"api1/v1/businesspartners/{bpCode}/documents?limit=50", ct);
            response.EnsureSuccessStatusCode();

            var rawDtos = await response.Content.ReadFromJsonAsync<List<SageX3DocumentDto>>(cancellationToken: ct);
            if (rawDtos == null) return new List<SageDocument>();

            return rawDtos.Select(SageMapper.MapToDomain).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sage X3 Gateway failure while fetching documents for partner {BpCode}", bpCode);
            throw;
        }
    }

    private async Task InjectBearerTokenHeaderAsync(CancellationToken ct)
    {
        string token = await _authService.GetAccessTokenAsync(ct);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
