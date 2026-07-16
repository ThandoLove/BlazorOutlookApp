using OperationalWorkspaceDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.IRepositories;
public interface IDocumentRepository
{
    Task<List<SageDocument>> GetByCustomerCodeAsync(string bpCode, CancellationToken ct);
    Task<SageDocument?> GetByNumberAsync(string docNumber, CancellationToken ct);
}
