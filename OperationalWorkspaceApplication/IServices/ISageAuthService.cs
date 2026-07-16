using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.IServices;


public interface ISageAuthService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
