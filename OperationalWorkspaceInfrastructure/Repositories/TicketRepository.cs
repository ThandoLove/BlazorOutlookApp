using Microsoft.EntityFrameworkCore;
using OperationalWorkspaceApplication.IRepositories;
using OperationalWorkspaceDomain.Entities;
using OperationalWorkspaceInfrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceInfrastructure.Repositories;
public class TicketRepository : ITicketRepository
{
    private readonly WorkspaceDbContext _context;
    public TicketRepository(WorkspaceDbContext context) => _context = context;

    public async Task AddAsync(OperationalTicket ticket, CancellationToken ct)
    {
        await _context.Tickets.AddAsync(ticket, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<OperationalTicket>> GetAllAsync(CancellationToken ct) =>
        await _context.Tickets.OrderByDescending(t => t.CreatedAt).AsNoTracking().ToListAsync(ct);

    public async Task<List<OperationalTicket>> GetByCustomerAsync(string customerId, CancellationToken ct) =>
        await _context.Tickets.Where(t => t.CustomerId == customerId).OrderByDescending(t => t.CreatedAt).AsNoTracking().ToListAsync(ct);
}