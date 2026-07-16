
using System;

namespace OperationalWorkspaceAPI.Logging;

public interface ITelemetryProcessor
{
    void TrackHttpRequest(string correlationId, string userId, string endpoint, string verb, int statusCode, long durationMs);
    void TrackErpIntegrationCall(string correlationId, string operation, string uri, bool isSuccess, long durationMs, string? errorPayload);
    void TrackSignalRCircuitMetric(string circuitId, string action, string metadata);
    object GetLivePerformanceDashboardMetrics();
}
