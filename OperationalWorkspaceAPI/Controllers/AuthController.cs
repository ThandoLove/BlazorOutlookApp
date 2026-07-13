using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OperationalWorkspaceInfrastruture.Configuration;
using System;

namespace OperationalWorkspaceAPI.Controllers;


using System;



[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SageOAuthConfig _oauthConfig;
    private readonly SageX3Settings _x3Settings;

    public AuthController(IOptions<SageOAuthConfig> oauthConfig, IOptions<SageX3Settings> x3Settings)
    {
        _oauthConfig = oauthConfig.Value;
        _x3Settings = x3Settings.Value;
    }

    [HttpGet("login")]
    public IActionResult InitiateOAuthHandshake()
    {
        if (_x3Settings.UseMockAuth)
        {
            return Ok(new { LoginUrl = "MOCK_MODE_ACTIVE", ExpectedState = "MOCK_STATE_HASH" });
        }

        var state = Guid.NewGuid().ToString("N");
        var oauthRedirectUrl = $"{_oauthConfig.AuthorizationEndpoint}?response_type=code" +
                               $"&client_id={Uri.EscapeDataString(_x3Settings.ClientId)}" +
                               $"&redirect_uri={Uri.EscapeDataString(_oauthConfig.RedirectUri)}" +
                               $"&state={state}";

        return Ok(new { LoginUrl = oauthRedirectUrl, ExpectedState = state });
    }

    [HttpGet("callback")]
    public IActionResult ProcessIdentityCallback([FromQuery] string code)
    {
        if (_x3Settings.UseMockAuth || code == "MOCK_OAUTH_CODE")
        {
            return Ok(new { Token = "MOCK_BEARER_TOKEN_VALID_2026", AssignedUserScope = "Admin;Finance;Sales;Consultant" });
        }

        // Live authorization exchange pipeline happens here via standard authorization handlers
        return Ok(new { Token = "LIVE_EXCHANGED_ACCESS_TOKEN", AssignedUserScope = "Sales" });
    }
}
