
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OperationalWorkspaceApplication.IServices;

namespace OperationalWorkspaceInfrastructure.SageX3.X3Diagnostics;

public class SageX3HealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ISageX3Configuration _config;

    public SageX3HealthCheck(HttpClient httpClient, ISageX3Configuration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_config.UseMocks)
        {
            return HealthCheckResult.Healthy("Sage X3 Integration layer is operating within simulated Mock development profiles.");
        }

        try
        {
            // Direct ping probe to check active server availability
            var response = await _httpClient.GetAsync("api1/v1/ping", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Connection handshake to Sage X3 Syracuse server succeeded.");
            }

            return HealthCheckResult.Unhealthy($"Sage X3 server returned an invalid response code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to establish a stable network socket connection to Sage X3 infrastructure.", ex);
        }
    }
}
