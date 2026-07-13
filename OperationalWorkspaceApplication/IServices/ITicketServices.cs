using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.Requests.TicketRequest;
using OperationalWorkspaceApplication.Responses.TicketResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.IServices;


public interface ITicketService
{
    Task<TicketActionResponse> HandleCreateTicketAsync(CreateTicketCommand command, CancellationToken ct);
    Task<AuditReportDto> GenerateGlobalSystemSummaryLogReportAsync(CancellationToken ct);
}
