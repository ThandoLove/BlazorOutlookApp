

using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.Requests.TicketRequest;
using OperationalWorkspaceApplication.Responses.TicketResponse;
using OperationalWorkspaceApplication.Responses.WorkspaceContextResponse;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using global::OperationalWorkspaceUI.UIState;
using System.Net.Http.Headers;

namespace OperationalWorkspaceUI.UIServices;

public class WorkspaceApiService : IWorkspaceApiService
{
    private readonly HttpClient _httpClient;
    private readonly UIStateContainer _stateContainer;

    public WorkspaceApiService(HttpClient httpClient, UIStateContainer stateContainer)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _stateContainer = stateContainer ?? throw new ArgumentNullException(nameof(stateContainer));
    }

    public async Task<OauthInitResponseDto?> InitializeOAuthChallengeAsync()
    {
        return await _httpClient.GetFromJsonAsync<OauthInitResponseDto>("api/auth/login");
    }

    public async Task<TokenExchangeResponseDto?> ExchangeTokenCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        return await _httpClient.GetFromJsonAsync<TokenExchangeResponseDto>($"api/auth/callback?code={Uri.EscapeDataString(code)}");
    }

    public async Task<WorkspaceContextResponse?> GetWorkspaceContextAsync(string email, string name, string activeUser, string userToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/customer/context?email={Uri.EscapeDataString(email)}&name={Uri.EscapeDataString(name)}&activeUser={Uri.EscapeDataString(activeUser)}");

        ApplySecurityHeaders(request, userToken);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<WorkspaceContextResponse>();
    }

    public async Task<TicketActionResponse?> SubmitIncidentTicketAsync(CreateTicketCommand command, string userToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/ticket")
        {
            Content = JsonContent.Create(command)
        };

        ApplySecurityHeaders(request, userToken);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            string rawError = await response.Content.ReadAsStringAsync();
            return new TicketActionResponse(false, string.Empty, $"Server processing fault: {rawError}", "Medium");
        }

        return await response.Content.ReadFromJsonAsync<TicketActionResponse>();
    }

    private void ApplySecurityHeaders(HttpRequestMessage request, string token)
    {
        // Inject Authorization Bearer context safely into outgoing threads
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Pass active security roles dynamically to support structural Authorization Policies evaluation
        request.Headers.Add("X-Sage-Functional-Roles", _stateContainer.UserRoleScope);
    }
}

