using OperationalWorkspaceDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.IRepositories;
public interface IAuditRepository
{
    Task LogBatchAsync(IEnumerable<AuditLog> entries, CancellationToken ct);
}
