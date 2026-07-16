using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OperationalWorkspaceInfrastructure.Configuration;
using OperationalWorkspaceApplication.IServices;
using OperationalWorkspaceDomain.Entities;

namespace OperationalWorkspaceInfrastructure.InfraServices;

public class SageX3GraphQLClient : ISageX3GraphQLClient
{
    private readonly HttpClient _httpClient;
    private readonly ISageAuthService _authService;
    private readonly SageX3Settings _settings;

    public SageX3GraphQLClient(HttpClient httpClient, ISageAuthService authService, IOptions<SageX3Settings> settings)
    {
        _httpClient = httpClient;
        _authService = authService;
        _settings = settings.Value;
    }

    public async Task<Customer?> FetchUnifiedCustomerContextAsync(string email, CancellationToken cancellationToken)
    {
        await InjectBearerAuthorizationHeaderAsync();

        var queryBlock = new
        {
            query = @"query GetBpContext($email: String!, $folder: String!) {
                businessPartner(email: $email, folder: $folder) {
                    bpCode, companyName, primaryContactName, statusFlag, creditLimitAmount, currentBalanceDue, salesRepIdentifier
                }
            }",
            variables = new { email = email.Trim().ToLowerInvariant(), folder = _settings.EndpointFolderName }
        };

        var response = await _httpClient.PostAsJsonAsync("api1/graphql", queryBlock, cancellationToken);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<GraphQlResponseEnvelope<BpQueryData>>(cancellationToken);
        var bpNode = envelope?.Data?.BusinessPartner;

        if (bpNode == null) return null;

        return Customer.CreateHydratedRecord(
            bpNode.BpCode, bpNode.CompanyName, bpNode.PrimaryContactName, email,
            bpNode.StatusFlag, bpNode.CreditLimitAmount, bpNode.CurrentBalanceDue, bpNode.SalesRepIdentifier);
    }

    public async Task<List<SageDocument>> FetchCustomerOrdersAndInvoicesAsync(string bpCode, CancellationToken cancellationToken)
    {
        await InjectBearerAuthorizationHeaderAsync();

        var queryBlock = new
        {
            query = @"query GetBpLedgerDocuments($bpCode: String!, $folder: String!) {
                salesOrders(customerCode: $bpCode, folder: $folder) {
                    documentId, documentType, creationDate, totalGrossAmount, currencyCode, lifecycleStatus, associatedBlobKey
                }
            }",
            variables = new { bpCode = bpCode.Trim().ToUpperInvariant(), folder = _settings.EndpointFolderName }
        };

        var response = await _httpClient.PostAsJsonAsync("api1/graphql", queryBlock, cancellationToken);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<GraphQlResponseEnvelope<OrdersQueryData>>(cancellationToken);
        if (envelope?.Data?.SalesOrders == null) return new List<SageDocument>();

        return envelope.Data.SalesOrders.Select(o => SageDocument.MapExternalRecord(
            o.DocumentId, o.DocumentType, o.CreationDate, o.TotalGrossAmount, o.CurrencyCode, o.LifecycleStatus, o.AssociatedBlobKey
        )).ToList();
    }

    private async Task InjectBearerAuthorizationHeaderAsync()
    {
        var token = await _authService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private class GraphQlResponseEnvelope<T> { [JsonPropertyName("data")] public T? Data { get; set; } }
    private class BpQueryData { [JsonPropertyName("businessPartner")] public BpGraphNode? BusinessPartner { get; set; } }
    private class OrdersQueryData { [JsonPropertyName("salesOrders")] public List<OrderGraphNode>? SalesOrders { get; set; } }

    private class BpGraphNode
    {
        [JsonPropertyName("bpCode")] public string BpCode { get; set; } = string.Empty;
        [JsonPropertyName("companyName")] public string CompanyName { get; set; } = string.Empty;
        [JsonPropertyName("primaryContactName")] public string PrimaryContactName { get; set; } = string.Empty;
        [JsonPropertyName("statusFlag")] public string StatusFlag { get; set; } = string.Empty;
        [JsonPropertyName("creditLimitAmount")] public decimal CreditLimitAmount { get; set; }
        [JsonPropertyName("currentBalanceDue")] public decimal CurrentBalanceDue { get; set; }
        [JsonPropertyName("salesRepIdentifier")] public string SalesRepIdentifier { get; set; } = string.Empty;
    }

    private class OrderGraphNode
    {
        [JsonPropertyName("documentId")] public string DocumentId { get; set; } = string.Empty;
        [JsonPropertyName("documentType")] public string DocumentType { get; set; } = string.Empty;
        [JsonPropertyName("creationDate")] public DateTime CreationDate { get; set; }
        [JsonPropertyName("totalGrossAmount")] public decimal TotalGrossAmount { get; set; }
        [JsonPropertyName("currencyCode")] public string CurrencyCode { get; set; } = string.Empty;
        [JsonPropertyName("lifecycleStatus")] public string LifecycleStatus { get; set; } = string.Empty;
        [JsonPropertyName("associatedBlobKey")] public string AssociatedBlobKey { get; set; } = string.Empty;
    }
}

