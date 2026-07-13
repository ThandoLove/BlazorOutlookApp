using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceInfrastruture.Configuration;


public class SageOAuthConfig
{
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string UserProfileEndpoint { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
