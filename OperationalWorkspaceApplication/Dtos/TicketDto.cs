using System;

namespace OperationalWorkspaceApplication.Dtos;

// FIX: Added the 'Status' string mapping parameter right before CreatedAt to take exactly 7 fields
public record TicketDto(
    string Id,
    string CustomerId,
    string Subject,
    string Description,
    string Priority,
    string Status,
    DateTime CreatedAt
);
