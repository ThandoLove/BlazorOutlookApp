using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.Requests.TicketRequest;
using OperationalWorkspaceApplication.Responses.TicketResponse;
using OperationalWorkspaceApplication.Responses.WorkspaceContextResponse;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using global::OperationalWorkspaceUI.UIState;
using Microsoft.Extensions.Logging;

namespace OperationalWorkspaceUI.UIServices;

public class WorkspaceApiService : IWorkspaceApiService
{
    private readonly HttpClient _httpClient;
    private readonly UIStateContainer _stateContainer;
    private readonly ILogger<WorkspaceApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public WorkspaceApiService(
        HttpClient httpClient,
        UIStateContainer stateContainer,
        ILogger<WorkspaceApiService> logger) // Phase 3 Audit: Native structural telemetry logging
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _stateContainer = stateContainer ?? throw new ArgumentNullException(nameof(stateContainer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Centralized Serialization Policies (Phase 3 Requirement #9)
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<OauthInitResponseDto?> InitializeOAuthChallengeAsync()
    {
        // FIX: Pointing to your unified v1 Auth entry-point routing pathways
        return await ExecuteWithClientResilienceAsync<OauthInitResponseDto>(
            "InitializeOAuthChallenge",
            () => _httpClient.GetAsync("api/v1/auth/login"));
    }

    public async Task<TokenExchangeResponseDto?> ExchangeTokenCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;

        // FIX: Pointing to your unified v1 Auth callback routing pathways
        return await ExecuteWithClientResilienceAsync<TokenExchangeResponseDto>(
            "ExchangeTokenCode",
            () => _httpClient.GetAsync($"api/v1/auth/callback?code={Uri.EscapeDataString(code)}"));
    }

    public async Task<WorkspaceContextResponse?> GetWorkspaceContextAsync(string email, string name, string activeUser, string userToken)
    {
        // FIX: Phase 4 Audit Resolution: Upgraded query path targeting resource-based versioned v1 REST endpoints
        string path = $"api/v1/customers/context?email={Uri.EscapeDataString(email)}&name={Uri.EscapeDataString(name)}&activeUser={Uri.EscapeDataString(activeUser)}";

        return await ExecuteWithClientResilienceAsync<WorkspaceContextResponse>(
            "GetWorkspaceContext",
            () => {
                var request = new HttpRequestMessage(HttpMethod.Get, path);
                ApplySecurityHeaders(request, userToken);
                return _httpClient.SendAsync(request);
            });
    }

    public async Task<TicketActionResponse?> SubmitIncidentTicketAsync(CreateTicketCommand command, string userToken)
    {
        if (command == null) return new TicketActionResponse(false, string.Empty, "Null payload submission.", "Medium");

        // FIX: Phase 4 Audit Resolution: Upgraded post path targeting versioned v1 plural REST collection routes
        return await ExecuteWithClientResilienceAsync<TicketActionResponse>(
            "SubmitIncidentTicket",
            async () => {
                var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/tickets")
                {
                    Content = JsonContent.Create(command, options: _jsonOptions)
                };
                ApplySecurityHeaders(request, userToken);
                return await _httpClient.SendAsync(request);
            });
    }

    private void ApplySecurityHeaders(HttpRequestMessage request, string token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Pass active security roles dynamically to support structural Authorization Policies evaluation
        request.Headers.Add("X-Sage-Functional-Roles", _stateContainer.UserRoleScope);
    }

    /// <summary>
    /// Phase 3 Requirement: Native Client-Side Resilience Engine.
    /// Executes outgoing Blazor gateway requests with Correlation IDs, Duration Timers, and Exponential Backoffs.
    /// </summary>
    private async Task<T?> ExecuteWithClientResilienceAsync<T>(string operation, Func<Task<HttpResponseMessage>> sendRequestFunc) where T : class
    {
        const int maxAttempts = 3;
        int currentAttempt = 0;
        int backoffMs = 1000;
        string correlationId = Guid.NewGuid().ToString()[..8].ToUpperInvariant();
        var timer = Stopwatch.StartNew();

        while (true)
        {
            currentAttempt++;
            try
            {
                _logger.LogInformation("[{TraceId}] UI Api Pipeline Request: {Op} (Attempt {Current}/{Max})",
                    correlationId, operation, currentAttempt, maxAttempts);

                var response = await sendRequestFunc();

                timer.Stop();
                _logger.LogInformation("[{TraceId}] UI Api Pipeline Response: {Op} returned HTTP {Code} in {Duration}ms",
                    correlationId, operation, (int)response.StatusCode, timer.ElapsedMilliseconds);

                if (!response.IsSuccessStatusCode)
                {
                    if (typeof(T) == typeof(TicketActionResponse))
                    {
                        string rawError = await response.Content.ReadAsStringAsync();
                        return new TicketActionResponse(false, string.Empty, $"Server processing fault: {rawError}", "Medium") as T;
                    }
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
            }
            catch (Exception ex) when (currentAttempt < maxAttempts && (ex is HttpRequestException || ex is TaskCanceledException))
            {
                _logger.LogWarning(ex, "[{TraceId}] Transient UI channel network stutter detected for '{Op}'. Pausing {Delay}ms before retry...",
                    correlationId, operation, backoffMs);

                await Task.Delay(backoffMs);
                backoffMs *= 2; // Exponential scale backoff loop
            }
            catch (Exception ex)
            {
                timer.Stop();
                _logger.LogError(ex, "[{TraceId}] Fatal client-side execution connection break inside UI operation: {Op}",
                    correlationId, operation);
                return null;
            }
        }
    }
}
