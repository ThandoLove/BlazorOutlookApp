using OperationalWorkspaceApplication.IServices;
using OperationalWorkspaceApplication.Services;

namespace OperationalWorkspaceInfrastructure.Configuration;

public class SageX3Settings : ISageX3Configuration, ITicketSystemConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string EndpointFolderName { get; set; } = "SEED";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public bool UseMocks { get; set; } = true;
    public bool UseMockAuth { get; set; } = true;
    public int HttpClientTimeoutSeconds { get; set; } = 15;

    // Phase 3 Audit: Maps the interface contract back to your standard JSON backing configuration variable property
    public string Tenant => EndpointFolderName;

    // Explicit conversion property maps to align both layers cleanly
    string ISageX3Configuration.BaseUrl => BaseUrl;
    string ISageX3Configuration.EndpointFolderName => EndpointFolderName;
    bool ISageX3Configuration.UseMocks => UseMocks;
    bool ISageX3Configuration.UseMockAuth => UseMockAuth;
    int ISageX3Configuration.HttpClientTimeoutSeconds => HttpClientTimeoutSeconds;
    string ISageX3Configuration.ClientId => ClientId;
    string ISageX3Configuration.ClientSecret => ClientSecret;

    bool ITicketSystemConfiguration.UseMocks => UseMocks;
}
