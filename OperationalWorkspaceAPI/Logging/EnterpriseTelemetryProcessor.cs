using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace OperationalWorkspaceAPI.Logging;

public class EnterpriseTelemetryProcessor : ITelemetryProcessor
{
    private readonly ILogger<EnterpriseTelemetryProcessor> _logger;

    // High-performance atomic telemetry accumulators
    private long _totalHttpRequestsTracked = 0;
    private long _totalErpHandshakesTracked = 0;
    private long _failedErpCallsCounter = 0;
    private long _activeSignalRCircuitsCounter = 0;

    private readonly ConcurrentQueue<string> _recentTelemetryAlerts = new();

    public EnterpriseTelemetryProcessor(ILogger<EnterpriseTelemetryProcessor> logger)
    {
        _logger = logger;
    }

    public void TrackHttpRequest(string correlationId, string userId, string endpoint, string verb, int statusCode, long durationMs)
    {
        Interlocked.Increment(ref _totalHttpRequestsTracked);

        // Section 5 Requirement: Production structured request tracking logging formatting (Correlation ID, Duration, Status)
        _logger.LogInformation(
            "[HTTP MONITOR] [{CorrelationId}] User: {User} | Route: {Verb} {Path} | Status: {Code} | Duration: {Duration}ms",
            correlationId, string.IsNullOrWhiteSpace(userId) ? "Anonymous" : userId, verb, endpoint, statusCode, durationMs);

        if (durationMs > 2000)
        {
            _recentTelemetryAlerts.Enqueue($"[{DateTime.UtcNow:T}] Latency Warning: {verb} {endpoint} took {durationMs}ms.");
            TrimAlertsQueue();
        }
    }

    public void TrackErpIntegrationCall(string correlationId, string operation, string uri, bool isSuccess, long durationMs, string? errorPayload)
    {
        Interlocked.Increment(ref _totalErpHandshakesTracked);
        if (!isSuccess) Interlocked.Increment(ref _failedErpCallsCounter);

        _logger.LogWarning(
            "[ERP GATEWAY] [{CorrelationId}] Op: {Operation} | Endpoint: {Uri} | Success: {Success} | Duration: {Duration}ms | Error: {Details}",
            correlationId, operation, uri, isSuccess, durationMs, errorPayload ?? "None");
    }

    public void TrackSignalRCircuitMetric(string circuitId, string action, string metadata)
    {
        if (action.Equals("CONNECT", StringComparison.OrdinalIgnoreCase))
            Interlocked.Increment(ref _activeSignalRCircuitsCounter);
        else if (action.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase))
            Interlocked.Decrement(ref _activeSignalRCircuitsCounter);

        _logger.LogInformation(
            "[SIGNALR CIRCUIT] Circuit: {CircuitId} | Action: {Action} | Meta: {Metadata} | Current Live Circuits: {ActiveCount}",
            circuitId, action, metadata, Interlocked.Read(ref _activeSignalRCircuitsCounter));
    }

    public object GetLivePerformanceDashboardMetrics()
    {
        return new
        {
            TotalIncomingHttpRequests = Interlocked.Read(ref _totalHttpRequestsTracked),
            TotalSageX3IntegrationCalls = Interlocked.Read(ref _totalErpHandshakesTracked),
            TotalFailedErpConnections = Interlocked.Read(ref _failedErpCallsCounter),
            ActiveLiveSignalRCircuits = Math.Max(0, Interlocked.Read(ref _activeSignalRCircuitsCounter)),
            TelemetryDiagnosticAlertsLog = _recentTelemetryAlerts.ToArray(),
            SystemReportingTimeUtc = DateTime.UtcNow
        };
    }

    private void TrimAlertsQueue()
    {
        while (_recentTelemetryAlerts.Count > 20)
        {
            _recentTelemetryAlerts.TryDequeue(out _);
        }
    }
}
