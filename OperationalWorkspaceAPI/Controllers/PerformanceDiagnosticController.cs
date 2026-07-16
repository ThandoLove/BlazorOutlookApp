
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OperationalWorkspaceAPI.Logging;
using System;

namespace OperationalWorkspaceAPI.Controllers;

[ApiController]
[Route("api/diagnostics")] // Isolated non-versioned corporate infrastructure monitoring route
public class PerformanceDiagnosticsController : ControllerBase
{
    private readonly ITelemetryProcessor _telemetryProcessor;

    public PerformanceDiagnosticsController(ITelemetryProcessor telemetryProcessor)
    {
        _telemetryProcessor = telemetryProcessor ?? throw new ArgumentNullException(nameof(telemetryProcessor));
    }

    /// <summary>
    /// Serves live runtime environment metrics to enterprise analytics platforms.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetLiveOperationalMetrics()
    {
        var analyticalSummary = _telemetryProcessor.GetLivePerformanceDashboardMetrics();
        return Ok(analyticalSummary);
    }
}
