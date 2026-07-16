using OperationalWorkspaceDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.IRepositories;
public interface ITicketRepository
{
    Task AddAsync(OperationalTicket ticket, CancellationToken ct);
    Task<List<OperationalTicket>> GetAllAsync(CancellationToken ct);
    Task<List<OperationalTicket>> GetByCustomerAsync(string customerId, CancellationToken ct);
}