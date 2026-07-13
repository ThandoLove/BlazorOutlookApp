using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceDomain.Entities;


public class OperationalTicket
{
    public string Id { get; private set; } = string.Empty;
    public string CustomerId { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Priority { get; private set; } = "Medium"; // Low, Medium, High, Critical
    public string Status { get; private set; } = "Open";    // Open, Investigating, Closed
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string OriginatingEmailId { get; private set; } = string.Empty;

    private OperationalTicket() { }

    public static OperationalTicket OpenNewWorkspaceIncident(
        string customerId, string subject, string description, string incomingEmailBody, string emailMessageId)
    {
        if (string.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("Incident logs require an associated Customer Code reference.");
        if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Summary action items require a subject descriptor statement.");

        // Non-boilerplate AI/Log routing prep tracker automation rule:
        string computedPriority = "Medium";
        string criticalContext = (subject + " " + description + " " + incomingEmailBody).ToUpperInvariant();

        if (criticalContext.Contains("LEGAL") || criticalContext.Contains("COMPLAINT") || criticalContext.Contains("BROKEN"))
        {
            computedPriority = "High";
        }
        if (criticalContext.Contains("LITIGATION") || criticalContext.Contains("LAWSUIT") || criticalContext.Contains("CRITICAL"))
        {
            computedPriority = "Critical";
        }

        return new OperationalTicket
        {
            Id = $"TK-TEMP-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}",
            CustomerId = customerId.Trim().ToUpperInvariant(),
            Subject = subject.Trim(),
            Description = description.Trim(),
            Priority = computedPriority,
            Status = "Open",
            CreatedAt = DateTime.UtcNow,
            OriginatingEmailId = emailMessageId ?? string.Empty
        };
    }

    public void UpdateProgressState(string newStatus)
    {
        if (newStatus != "Open" && newStatus != "Investigating" && newStatus != "Closed")
            throw new InvalidOperationException("Illegal transition configuration state parameters.");

        Status = newStatus;
    }

    public void AssignFinalDatabaseIdentityToken(string finalizedDbId)
    {
        if (!Id.StartsWith("TK-TEMP-")) throw new InvalidOperationException("Identity signatures already permanently frozen.");
        Id = finalizedDbId;
    }
}
