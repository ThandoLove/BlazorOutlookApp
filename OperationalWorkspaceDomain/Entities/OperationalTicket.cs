using OperationalWorkspaceDomain.Enums;
using OperationalWorkspaceDomain.Enums.TicketsEnum;
using OperationalWorkspaceDomain.Strategies;
using System;

namespace OperationalWorkspaceDomain.Entities;

public class OperationalTicket
{
    public string Id { get; private set; } = string.Empty;
    public string CustomerId { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string EmailBody { get; private set; } = string.Empty;
    public string EmailMessageId { get; private set; } = string.Empty;

    // Rich Enum structural fields replacing fragile strings
    public TicketPriority Priority { get; private set; }
    public TicketStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Private Constructor prevents parameter bypass mutations
    private OperationalTicket() { }

    public static OperationalTicket OpenNewWorkspaceIncident(
        string customerId,
        string subject,
        string description,
        string emailBody,
        string emailMessageId,
        ITicketPriorityStrategy priorityPolicy) // Enforces policy injection dependency invariant
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Incident logs require a valid Customer reference tracking token.", nameof(customerId));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Incident title summary item cannot be empty.", nameof(subject));

        ArgumentNullException.ThrowIfNull(priorityPolicy);

        var calculatedPriority = priorityPolicy.DeterminePriority(subject, description, emailBody);

        return new OperationalTicket
        {
            Id = $"PENDING-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}",
            CustomerId = customerId.Trim().ToUpperInvariant(),
            Subject = subject.Trim(),
            Description = description?.Trim() ?? string.Empty,
            EmailBody = emailBody?.Trim() ?? string.Empty,
            EmailMessageId = emailMessageId?.Trim() ?? string.Empty,
            Priority = calculatedPriority,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AssignFinalDatabaseIdentityToken(string finalizedId)
    {
        if (string.IsNullOrWhiteSpace(finalizedId))
            throw new InvalidOperationException("Cannot assign an empty persistence identifier.");

        Id = finalizedId;
    }
}
