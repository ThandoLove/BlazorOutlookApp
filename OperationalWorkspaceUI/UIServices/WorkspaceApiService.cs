using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.Requests.TicketRequest;
using OperationalWorkspaceApplication.Responses.TicketResponse;
using OperationalWorkspaceApplication.Responses.WorkspaceContextResponse;
using OperationalWorkspaceUI.Models; // Reference the unified Result wrapper
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using global::OperationalWorkspaceUI.UIState;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebUtilities;

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
        ILogger<WorkspaceApiService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _stateContainer = stateContainer ?? throw new ArgumentNullException(nameof(stateContainer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<Result<OauthInitResponseDto>> InitializeOAuthChallengeAsync()
    {
        var rawResponse = await ExecuteWithClientResilienceAsync<OauthInitResponseDto>(
            "InitializeOAuthChallenge",
            () => _httpClient.GetAsync("api/v1/auth/login"));

        if (rawResponse == null)
        {
            return Result<OauthInitResponseDto>.Failure("Failed to initialize the external authentication sequence.", "AUTH_INIT_FAILED");
        }

        return Result<OauthInitResponseDto>.Success(rawResponse);
    }

    public async Task<Result<TokenExchangeResponseDto>> ExchangeTokenCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<TokenExchangeResponseDto>.Failure("Authorization callback code argument cannot be blank.", "INVALID_ARGUMENT");
        }

        var queryParams = new Dictionary<string, string?> { { "code", code } };
        string path = QueryHelpers.AddQueryString("api/v1/auth/callback", queryParams);

        var rawResponse = await ExecuteWithClientResilienceAsync<TokenExchangeResponseDto>(
            "ExchangeTokenCode",
            () => _httpClient.GetAsync(path));

        if (rawResponse == null)
        {
            return Result<TokenExchangeResponseDto>.Failure("Syracuse identity provider rejected token exchange payload code.", "TOKEN_EXCHANGE_REJECTED");
        }

        return Result<TokenExchangeResponseDto>.Success(rawResponse);
    }

    public async Task<Result<WorkspaceContextResponse>> GetWorkspaceContextAsync(string email, string name, string activeUser, string userToken)
    {
        var queryParams = new Dictionary<string, string?>
        {
            { "email", email },
            { "name", name },
            { "activeUser", activeUser }
        };
        string path = QueryHelpers.AddQueryString("api/v1/customers/context", queryParams);

        var rawResponse = await ExecuteWithClientResilienceAsync<WorkspaceContextResponse>(
            "GetWorkspaceContext",
            () => {
                var request = new HttpRequestMessage(HttpMethod.Get, path);
                ApplySecurityHeaders(request, userToken);
                return _httpClient.SendAsync(request);
            });

        if (rawResponse == null)
        {
            return Result<WorkspaceContextResponse>.Failure("Identity context could not be resolved from active Sage folders.", "CUSTOMER_NOT_FOUND");
        }

        return Result<WorkspaceContextResponse>.Success(rawResponse);
    }

    public async Task<Result<TicketActionResponse>> SubmitIncidentTicketAsync(CreateTicketCommand command, string userToken)
    {
        if (command == null)
        {
            return Result<TicketActionResponse>.Failure("Null transaction payload block submitted.", "INVALID_PAYLOAD");
        }

        var rawResponse = await ExecuteWithClientResilienceAsync<TicketActionResponse>(
            "SubmitIncidentTicket",
            async () => {
                var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/tickets")
                {
                    Content = JsonContent.Create(command, options: _jsonOptions)
                };
                ApplySecurityHeaders(request, userToken);
                return await _httpClient.SendAsync(request);
            });

        if (rawResponse == null)
        {
            return Result<TicketActionResponse>.Failure("System encountered an internal processing fault while creating the support incident.", "TICKET_CREATION_FAILED");
        }

        return Result<TicketActionResponse>.Success(rawResponse);
    }

    private void ApplySecurityHeaders(HttpRequestMessage request, string token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        request.Headers.Add("X-Sage-Functional-Roles", _stateContainer.UserRoleScope);
    }

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

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("[{TraceId}] Security boundary detected an expired or revoked session token. Forcing safe clear of state stores...", correlationId);
                    _stateContainer.ClearSessionStore();
                    break;
                }

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
            }
            catch (Exception ex) when (currentAttempt < maxAttempts && (ex is HttpRequestException || ex is TaskCanceledException))
            {
                _logger.LogWarning(ex, "[{TraceId}] Transient UI channel network stutter detected for '{Op}'. Pausing {Delay}ms before retry...",
                    correlationId, operation, backoffMs);

                await Task.Delay(backoffMs);
                backoffMs *= 2;
            }
            catch (Exception ex)
            {
                timer.Stop();
                _logger.LogError(ex, "[{TraceId}] Fatal client-side execution connection break inside UI operation: {Op}",
                    correlationId, operation);
                return null;
            }
        }

        return null;
    }
}
