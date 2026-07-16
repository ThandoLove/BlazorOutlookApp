using OperationalWorkspaceApplication.IRepositories;
using OperationalWorkspaceDomain.Entities;
using OperationalWorkspaceInfrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceInfrastructure.Repositories;
public class AuditRepository : IAuditRepository
{
    private readonly WorkspaceDbContext _context;
    public AuditRepository(WorkspaceDbContext context) => _context = context;

    public async Task LogBatchAsync(IEnumerable<AuditLog> entries, CancellationToken ct)
    {
        await _context.AuditLogs.AddRangeAsync(entries, ct);
        await _context.SaveChangesAsync(ct);
    }
}