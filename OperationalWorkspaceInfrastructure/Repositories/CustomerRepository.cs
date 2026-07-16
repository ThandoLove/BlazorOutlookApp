using OperationalWorkspaceApplication.IRepositories;
using OperationalWorkspaceDomain.Entities;
using OperationalWorkspaceInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OperationalWorkspaceInfrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly WorkspaceDbContext _context;
    public CustomerRepository(WorkspaceDbContext context) => _context = context;

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct) =>
        await _context.Customers.FirstOrDefaultAsync(c => c.Email == email, ct);

    public async Task SaveAsync(Customer customer, CancellationToken ct)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync(ct);
    }
}
