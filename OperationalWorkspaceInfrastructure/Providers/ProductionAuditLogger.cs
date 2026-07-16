using Microsoft.Extensions.Logging;
using OperationalWorkspaceApplication.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;


namespace OperationalWorkspaceInfrastructure.Providers;

public class ProductionAuditLogger : IAuditLogger
{
    private readonly ILogger<ProductionAuditLogger> _logger;
    public ProductionAuditLogger(ILogger<ProductionAuditLogger> logger) => _logger = logger;

    public void LogIntegrationMetric(string correlationId, string operation, string endpoint, int statusCode, long durationMs, string? errorDetails)
    {
        // Capture mandatory telemetry parameters: Correlation ID, Timestamp, Endpoint, Status, Duration, and filtered Error arrays
        _logger.LogInformation(
            "[{CorrelationId}] Timestamp: {Timestamp} | Op: {Operation} | Uri: {Endpoint} | Status: {Status} | Took: {Duration}ms | Error: {Details}",
            correlationId, DateTime.UtcNow.ToString("O"), operation, endpoint, statusCode, durationMs, errorDetails ?? "None");
    }
}