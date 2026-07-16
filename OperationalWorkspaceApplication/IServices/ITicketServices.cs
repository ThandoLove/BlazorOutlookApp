using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.Requests.TicketRequest;
using OperationalWorkspaceApplication.Responses.TicketResponse;


namespace OperationalWorkspaceApplication.IServices;


public interface ITicketService
{
    Task<TicketActionResponse> HandleCreateTicketAsync(CreateTicketCommand command, CancellationToken ct);
    Task<AuditReportDto> GenerateGlobalSystemSummaryLogReportAsync(CancellationToken ct);
}
