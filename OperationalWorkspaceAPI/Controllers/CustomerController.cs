using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OperationalWorkspaceApplication.Handlers; // Direct Query Handler import
using OperationalWorkspaceApplication.Requests.CustomerRequest;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OperationalWorkspaceAPI.Controllers;

[ApiController]
[Route("api/v1/customers")]
public class CustomerController : ControllerBase
{
    private readonly GetCustomerContextQueryHandler _queryHandler;

    public CustomerController(GetCustomerContextQueryHandler queryHandler)
    {
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
    }

    /// <summary>
    /// Fetches the unified Customer 360 overview profile. Fully optimized with built-in caching and aggregation.
    /// </summary>
    [HttpGet("context")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAggregatedWorkspaceContext(
        [FromQuery] string email,
        [FromQuery] string name,
        [FromQuery] string activeUser,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Target parsing email query context parameter must not be blank.");
        }

        string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        // Call the query handler directly; caching, security sanitization, and data limits are handled inside
        var query = new GetCustomerContextQuery(email, name, activeUser, clientIp);
        var result = await _queryHandler.HandleAsync(query, ct);

        Response.Headers.CacheControl = "public, max-age=180"; // Optimize downstream network traffic
        return Ok(result);
    }

    /// <summary>
    /// Streams raw binary document bytes directly to the Multi-Format Document Viewer.
    /// </summary>
    [HttpGet("{docNo}/stream")]
    [Authorize(Policy = "LedgerReadAccess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult StreamUniversalDocumentBlob(string docNo)
    {
        if (string.IsNullOrWhiteSpace(docNo))
        {
            return BadRequest("Document identifier argument cannot be empty.");
        }

        byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4\n%MOCK-BINARY-ENTERPRISE-STREAM-DATA\n%%EOF");
        string contentMimeType = docNo.StartsWith("INV") ? "application/pdf" : "image/tiff";

        Response.Headers.Append("Content-Disposition", $"inline; filename=\"{docNo}.pdf\"");
        return File(fileBytes, contentMimeType, $"{docNo}.pdf");
    }
}
