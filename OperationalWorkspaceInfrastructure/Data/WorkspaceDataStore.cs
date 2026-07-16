using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OperationalWorkspaceApplication.AppData; // Pulls your clean Application interface contract
using OperationalWorkspaceDomain.Entities;

namespace OperationalWorkspaceInfrastructure.Data;

public class WorkspaceDataStore : IWorkspaceDataStore
{
    private readonly WorkspaceDbContext _context;

    public WorkspaceDataStore(WorkspaceDbContext context)
    {
        _context = context;
    }

    public async Task AddTicketAsync(OperationalTicket ticket, CancellationToken ct)
    {
        await _context.Tickets.AddAsync(ticket, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<OperationalTicket>> GymAllTicketsAsync(CancellationToken ct)
    {
        return await _context.Tickets.OrderByDescending(t => t.CreatedAt).AsNoTracking().ToListAsync(ct);
    }

    public async Task<List<OperationalTicket>> GetTicketsByCustomerAsync(string customerId, CancellationToken ct)
    {
        return await _context.Tickets
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task AddAuditLogBatchAsync(IEnumerable<AuditLog> logs, CancellationToken ct)
    {
        await _context.AuditLogs.AddRangeAsync(logs, ct);
        await _context.SaveChangesAsync(ct);
    }
}
