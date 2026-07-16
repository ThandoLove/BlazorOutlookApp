using OperationalWorkspaceDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OperationalWorkspaceApplication.IRepositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct);
    Task SaveAsync(Customer customer, CancellationToken ct);
}