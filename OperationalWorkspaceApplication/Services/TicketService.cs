using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.IServices;
using OperationalWorkspaceApplication.Mappers;
using OperationalWorkspaceApplication.Requests.TicketRequest;
using OperationalWorkspaceApplication.Responses.TicketResponse;
using OperationalWorkspaceDomain.Entities;
using OperationalWorkspaceInfrastruture.Configuration;
using OperationalWorkspaceInfrastruture.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace OperationalWorkspaceApplication.Services;

public class TicketService : ITicketService
{
    private readonly WorkspaceDbContext _dbContext;
    private readonly TicketPersistenceOptions _fileOptions;
    private readonly SageX3Settings _settings;
    private static readonly object _fileLock = new();

    public TicketService(
        WorkspaceDbContext dbContext,
        IOptions<TicketPersistenceOptions> fileOptions,
        IOptions<SageX3Settings> settings)
    {
        _dbContext = dbContext;
        _fileOptions = fileOptions.Value;
        _settings = settings.Value;
    }

    public async Task<TicketActionResponse> HandleCreateTicketAsync(CreateTicketCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var ticketEntity = OperationalTicket.OpenNewWorkspaceIncident(
                command.CustomerCode, command.Subject, command.Description, command.EmailBody, command.EmailMessageId
            );

            if (_settings.UseMocks)
            {
                lock (_fileLock)
                {
                    // Fallback to locally isolated JSON isolated persistence when working without databases
                    if (!Directory.Exists(_fileOptions.DatabaseDirectory)) Directory.CreateDirectory(_fileOptions.DatabaseDirectory);

                    var list = new List<OperationalTicket>();
                    if (File.Exists(_fileOptions.FullDatabasePath))
                    {
                        var text = File.ReadAllText(_fileOptions.FullDatabasePath);
                        list = JsonSerializer.Deserialize<List<OperationalTicket>>(text) ?? new();
                    }

                    ticketEntity.AssignFinalDatabaseIdentityToken($"TK-FILE-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}");
                    list.Add(ticketEntity);
                    File.WriteAllText(_fileOptions.FullDatabasePath, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
                }
                return new TicketActionResponse(true, ticketEntity.Id, string.Empty, ticketEntity.Priority);
            }

            // Database Live Path
            ticketEntity.AssignFinalDatabaseIdentityToken($"TK-SQL-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}");
            await _dbContext.Tickets.AddAsync(ticketEntity, ct);
            await _dbContext.SaveChangesAsync(ct);

            return new TicketActionResponse(true, ticketEntity.Id, string.Empty, ticketEntity.Priority);
        }
        catch (Exception ex)
        {
            return new TicketActionResponse(false, string.Empty, ex.Message, "Medium");
        }
    }

    public async Task<AuditReportDto> GenerateGlobalSystemSummaryLogReportAsync(CancellationToken ct)
    {
        var outList = new List<OperationalTicket>();

        if (_settings.UseMocks)
        {
            lock (_fileLock)
            {
                if (File.Exists(_fileOptions.FullDatabasePath))
                {
                    var text = File.ReadAllText(_fileOptions.FullDatabasePath);
                    outList = JsonSerializer.Deserialize<List<OperationalTicket>>(text) ?? new();
                }
            }
        }
        else
        {
            outList = await _dbContext.Tickets.OrderByDescending(t => t.CreatedAt).AsNoTracking().ToListAsync(ct);
        }

        int highPriorityCount = outList.Count(t => t.Priority == "High" || t.Priority == "Critical");
        int openCount = outList.Count(t => t.Status == "Open");
        var dtos = outList.Select(IdentityModelMapper.MapToDto).ToList();

        return new AuditReportDto(outList.Count, openCount, highPriorityCount, DateTime.UtcNow, dtos);
    }
}
