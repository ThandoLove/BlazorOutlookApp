using OperationalWorkspaceInfrastruture.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OperationalWorkspaceInfrastructure.SecurityInfra;

namespace OperationalWorkspaceInfrastruture.InfraServices;


public interface ISageAuthService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}

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
                throw new InvalidOperationException("Empty authentication mapping context received from Syracuse token server.");

            var protectedToken = _encryptionProvider.ProtectSensitivePayload(tokenDto.AccessToken);
            var cacheConfig = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(tokenDto.ExpiresIn - 45)); // Buffer safety window

            _memoryCache.Set(CacheKey, protectedToken, cacheConfig);

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
