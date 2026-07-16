using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Abstractions;
public interface IAuditLogger
{
    void LogIntegrationMetric(string correlationId, string operation, string endpoint, int statusCode, long durationMs, string? errorDetails);
}