using Microsoft.AspNetCore.Mvc;

namespace OperationalWorkspaceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    [HttpGet("verify-sender")]
    public IActionResult VerifySenderProfileAffiliation([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return BadRequest("Target tracking email context parameter is blank.");

        // Rule: Detect internal corporate structures cleanly without database hits
        bool isStaff = email.EndsWith("@yourcompany.com", System.StringComparison.OrdinalIgnoreCase);

        return Ok(new
        {
            EmailAddress = email,
            IsInternalEmployee = isStaff,
            AffiliationGroupMarker = isStaff ? "Internal Corporate User Team" : "External Account Entity"
        });
    }
}
