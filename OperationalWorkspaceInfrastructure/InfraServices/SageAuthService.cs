using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OperationalWorkspaceApplication.AppSecurity;
using OperationalWorkspaceApplication.IServices;
using OperationalWorkspaceInfrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace OperationalWorkspaceInfrastructure.InfraServices;

public class SageAuthService : ISageAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ITokenEncryptionProvider _encryptionProvider;
    private readonly SageX3Settings _settings;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private const string CacheKey = "SageX3_GraphQL_ConnectedApp_Token";

    public SageAuthService(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        ITokenEncryptionProvider encryptionProvider,
        IOptions<SageX3Settings> settings)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _encryptionProvider = encryptionProvider;
        _settings = settings.Value;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_settings.UseMockAuth) return "MOCK_BEARER_TOKEN_VALID_2026";

        // Double-checked locking via thread-safe Semaphore to handle high concurrent task pane initialization bursts
        if (_memoryCache.TryGetValue(CacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            return _encryptionProvider.UnprotectSensitivePayload(cachedToken);
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_memoryCache.TryGetValue(CacheKey, out string? directToken) && !string.IsNullOrEmpty(directToken))
            {
                return _encryptionProvider.UnprotectSensitivePayload(directToken);
            }

            // Extract secrets directly from OS environment blocks instead of configuration text files
            string clientId = _encryptionProvider.ExtractSecureMachineCredential("SageX3Settings__ClientId");
            string clientSecret = _encryptionProvider.ExtractSecureMachineCredential("SageX3Settings__ClientSecret");

            var tokenRequestPayload = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "scope", "api" }
            });

            var response = await _httpClient.PostAsync("/token", tokenRequestPayload, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tokenDto = await response.Content.ReadFromJsonAsync<TokenEnvelopeDto>(cancellationToken: cancellationToken);
            if (tokenDto == null || string.IsNullOrEmpty(tokenDto.AccessToken))
                throw new InvalidOperationException("Empty authentication mapping context received from Syracuse token endpoint.");

            var securedValue = _encryptionProvider.ProtectSensitivePayload(tokenDto.AccessToken);

            // Phase 3 Audit Resolution: Deduct a 45-second buffer window to automatically renew the session BEFORE it fails
            var secureLifespan = TimeSpan.FromSeconds(Math.Max(30, tokenDto.ExpiresIn - 45));
            var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(secureLifespan);

            _memoryCache.Set(CacheKey, securedValue, cacheOptions);

            return tokenDto.AccessToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private class TokenEnvelopeDto
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
    }
}
