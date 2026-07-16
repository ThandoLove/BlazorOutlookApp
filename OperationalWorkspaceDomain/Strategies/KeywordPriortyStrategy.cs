using OperationalWorkspaceDomain.Enums.TicketsEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceDomain.Strategies;

public class KeywordPriorityStrategy : ITicketPriorityStrategy
{
    private static readonly string[] CriticalKeywords = { "OUTAGE", "CRITICAL", "DOWN", "EMERGENCY", "DISCREPANCY" };
    private static readonly string[] HighKeywords = { "URGENT", "DISPUTE", "ERROR", "BLOCKED" };

    public TicketPriority DeterminePriority(string subject, string description, string emailBody)
    {
        var combinedText = $"{subject} {description} {emailBody}".ToUpperInvariant();

        foreach (var keyword in CriticalKeywords)
        {
            if (combinedText.Contains(keyword)) return TicketPriority.Critical;
        }

        foreach (var keyword in HighKeywords)
        {
            if (combinedText.Contains(keyword)) return TicketPriority.High;
        }

        return TicketPriority.Medium;
    }
}
