using OperationalWorkspaceDomain.Entities;
using System.Collections.Generic;
using System.Threading;



namespace OperationalWorkspaceApplication.IServices;

public interface ISageX3GraphQLClient
{
    Task<Customer?> FetchUnifiedCustomerContextAsync(string email, CancellationToken cancellationToken);
    Task<List<SageDocument>> FetchCustomerOrdersAndInvoicesAsync(string bpCode, CancellationToken cancellationToken);
}
