using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OperationalWorkspaceAPI.Controllers;

[ApiController]
[Route("health")] // Non-versioned system diagnostic lifecycle endpoint
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CheckSystemHealth(CancellationToken ct)
    {
        // Executes all registered IHealthCheck classes (including your SageX3HealthCheck)
        var report = await _healthCheckService.CheckHealthAsync(ct);

        var healthSummary = new
        {
            Status = report.Status.ToString(),
            CompiledVersion = "1.0.0-Beta-v4",
            TimestampUtc = DateTime.UtcNow,
            MachineEnvironment = Environment.MachineName
        };

        if (report.Status == HealthStatus.Unhealthy)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, healthSummary);
        }

        return Ok(healthSummary);
    }
}
