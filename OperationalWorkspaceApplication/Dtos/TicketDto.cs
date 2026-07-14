

namespace OperationalWorkspaceApplication.Dtos;


public record TicketDto(string Id, string CustomerId, string Subject, string Description, string Priority, string Status, DateTime CreatedAt);
