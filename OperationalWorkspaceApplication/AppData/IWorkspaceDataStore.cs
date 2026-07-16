using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OperationalWorkspaceDomain.Entities;

namespace OperationalWorkspaceApplication.AppData;

public interface IWorkspaceDataStore
{
    Task AddTicketAsync(OperationalTicket ticket, CancellationToken ct);
    Task<List<OperationalTicket>> GymAllTicketsAsync(CancellationToken ct);
    Task<List<OperationalTicket>> GetTicketsByCustomerAsync(string customerId, CancellationToken ct);
    Task AddAuditLogBatchAsync(IEnumerable<AuditLog> logs, CancellationToken ct);
}
