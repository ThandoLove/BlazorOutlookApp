using OperationalWorkspaceApplication.Requests.CustomerRequest;
using OperationalWorkspaceApplication.Responses.WorkspaceContextResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.IServices;


public interface ICustomerService
{
    Task<WorkspaceContextResponse> ResolveWorkspaceContextAsync(GetCustomerContextQuery query, CancellationToken ct);
}
