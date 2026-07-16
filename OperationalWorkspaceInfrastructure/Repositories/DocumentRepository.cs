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
public class DocumentRepository : IDocumentRepository
{
    private readonly WorkspaceDbContext _context;
    public DocumentRepository(WorkspaceDbContext context) => _context = context;

    public async Task<List<SageDocument>> GetByCustomerCodeAsync(string bpCode, CancellationToken ct) =>
        await _context.Documents.Where(d => d.CustomerId == bpCode).AsNoTracking().ToListAsync(ct);

    public async Task<SageDocument?> GetByNumberAsync(string docNumber, CancellationToken ct) =>
        await _context.Documents.FirstOrDefaultAsync(d => d.DocumentNumber == docNumber, ct);
}