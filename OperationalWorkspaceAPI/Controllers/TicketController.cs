using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OperationalWorkspaceApplication.IServices;
using OperationalWorkspaceApplication.Requests.TicketRequest;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OperationalWorkspaceAPI.Controllers;




[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
    }

    /// <summary>
    /// Spins up a new customer incident, task, or note directly inside the tracking data store.
    /// Protected by PipelineWriteAccess Authorization Policies.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "PipelineWriteAccess")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> OpenNewIncidentTicket([FromBody] CreateTicketCommand command, CancellationToken ct)
    {
        if (command == null)
        {
            return BadRequest("Invalid or unparseable ticket ingestion payload.");
        }

        var result = await _ticketService.HandleCreateTicketAsync(command, ct);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ValidationErrorMessage);
        }

        // Return a professional Created routing descriptor pointing to your reporting resource path
        return CreatedAtAction(
            nameof(GetGlobalSystemSummaryReport),
            new { id = result.TicketId },
            result
        );
    }

    /// <summary>
    /// Compiles a collective system summary log log audit trail.
    /// Protected by RegistryAdminAccess Authorization Policies.
    /// </summary>
    [HttpGet("report/summary")]
    [Authorize(Policy = "RegistryAdminAccess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetGlobalSystemSummaryReport(CancellationToken ct)
    {
        var reportSummary = await _ticketService.GenerateGlobalSystemSummaryLogReportAsync(ct);
        return Ok(reportSummary);
    }
}
