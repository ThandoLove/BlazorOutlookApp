using Microsoft.Extensions.Options;
using OperationalWorkspaceApplication.AppData;
using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.Exceptions;
using OperationalWorkspaceApplication.IServices;
using OperationalWorkspaceApplication.Mappers;
using OperationalWorkspaceApplication.Requests.TicketRequest;
using OperationalWorkspaceApplication.Responses.TicketResponse;

using OperationalWorkspaceDomain.Entities;
using OperationalWorkspaceDomain.Enums;
using OperationalWorkspaceDomain.Enums.TicketsEnum;
using OperationalWorkspaceDomain.Strategies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Services;

public class TicketService : ITicketService
{
    private readonly IWorkspaceDataStore _dataStore;
    private readonly ITicketPersistenceConfiguration _fileOptions;
    private readonly ITicketSystemConfiguration _settings;
    private readonly ITicketPriorityStrategy _priorityPolicy; // Injected priority evaluator
    private static readonly object _fileLock = new();

    public TicketService(
        IWorkspaceDataStore dataStore,
        ITicketPersistenceConfiguration fileOptions,
        ITicketSystemConfiguration settings,
        ITicketPriorityStrategy priorityPolicy)
    {
        _dataStore = dataStore;
        _fileOptions = fileOptions;
        _settings = settings;
        _priorityPolicy = priorityPolicy;
    }

    public async Task<TicketActionResponse> HandleCreateTicketAsync(CreateTicketCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            // Execute aggregate builder using clean extracted strategy rules
            var ticketEntity = OperationalTicket.OpenNewWorkspaceIncident(
                command.CustomerCode, command.Subject, command.Description, command.EmailBody, command.EmailMessageId, _priorityPolicy
            );

            if (_settings.UseMocks)
            {
                lock (_fileLock)
                {
                    if (!Directory.Exists(_fileOptions.DatabaseDirectory))
                        Directory.CreateDirectory(_fileOptions.DatabaseDirectory);

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
                return new TicketActionResponse(true, ticketEntity.Id, string.Empty, ticketEntity.Priority.ToString());
            }

            ticketEntity.AssignFinalDatabaseIdentityToken($"TK-SQL-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}");
            await _dataStore.AddTicketAsync(ticketEntity, ct);

            return new TicketActionResponse(true, ticketEntity.Id, string.Empty, ticketEntity.Priority.ToString());
        }
        catch (Exception ex)
        {
            // Wrap legacy exceptions inside your domain exception tree definitions safely
            throw new TicketCreationException(ex.Message, ex);
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
            outList = await _dataStore.GymAllTicketsAsync(ct);
        }

        int highPriorityCount = outList.Count(t => t.Priority == TicketPriority.High || t.Priority == TicketPriority.Critical);
        int openCount = outList.Count(t => t.Status == TicketStatus.Open);
        var dtos = outList.Select(IdentityModelMapper.MapToDto).ToList();

        return new AuditReportDto(outList.Count, openCount, highPriorityCount, DateTime.UtcNow, dtos);
    }
}
