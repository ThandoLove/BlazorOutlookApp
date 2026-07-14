using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceInfrastruture.Configuration;


public class SageX3Settings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string EndpointFolderName { get; set; } = "SEED";
    public bool UseMocks { get; set; } = true;
    public bool UseMockAuth { get; set; } = true;
    public int HttpClientTimeoutSeconds { get; set; } = 15;
 
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
