using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using OperationalWorkspaceApplication.IServices;
using OperationalWorkspaceApplication.Requests.CustomerRequest;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace OperationalWorkspaceAPI.Controllers;


[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IMemoryCache _memoryCache;

    public CustomerController(ICustomerService customerService, IMemoryCache memoryCache)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    /// <summary>
    /// Fetches the unified Customer 360 overview profile. Intercepted by memory caching layers.
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
        string cacheKey = $"Workspace_Context_Cache_{email.Trim().ToLowerInvariant()}";

        // Interceptor: Check server memory cache to prevent redundant loops hitting live Sage schemas
        if (_memoryCache.TryGetValue(cacheKey, out var cachedResponse))
        {
            Response.Headers.Append("X-Cache-Status", "HIT_SERVER_MEMORY");
            return Ok(cachedResponse);
        }

        var query = new GetCustomerContextQuery(email, name, activeUser, clientIp);
        var result = await _customerService.ResolveWorkspaceContextAsync(query, ct);

        if (result == null)
        {
            return NotFound("Identity context matches no active records inside destination Sage folder pathways.");
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        _memoryCache.Set(cacheKey, result, cacheEntryOptions);

        Response.Headers.Append("X-Cache-Status", "MISS_FETCHED_LIVE");
        Response.Headers.CacheControl = "public, max-age=300"; // Instruction header for Outlook browser containers

        return Ok(result);
    }

    /// <summary>
    /// Streams raw binary document bytes directly to the Blazor Multi-Format Document Viewer and Attachment Engine.
    /// Protected by LedgerReadAccess Authorization Policies.
    /// </summary>
    [HttpGet("documents/{docNo}/stream")]
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

        // Multipurpose Document Viewer Engine Stream: Simulate compiling arbitrary mime streams safely.
        // In live systems, this streams a direct byte array pull from your infrastructure layer's Sage client.
        byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4\n%MOCK-BINARY-ENTERPRISE-STREAM-DATA\n%%EOF");

        string contentMimeType = docNo.StartsWith("INV") ? "application/pdf" : "image/tiff";

        Response.Headers.Append("Content-Disposition", $"inline; filename=\"{docNo}.pdf\"");
        return File(fileBytes, contentMimeType, $"{docNo}.pdf");
    }
}
