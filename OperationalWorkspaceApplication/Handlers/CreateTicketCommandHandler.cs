using OperationalWorkspaceApplication.Abstractions;
using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.Exceptions;
using OperationalWorkspaceApplication.IRepositories;
using OperationalWorkspaceApplication.IServices;
using OperationalWorkspaceApplication.Requests.TicketRequest;
using OperationalWorkspaceApplication.Responses.TicketResponse;
using OperationalWorkspaceApplication.Services;
using OperationalWorkspaceDomain.Entities;
using OperationalWorkspaceDomain.Strategies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Handlers;

public class CreateTicketCommandHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketPersistenceConfiguration _fileOptions;
    private readonly ITicketSystemConfiguration _settings;
    private readonly ITicketPriorityStrategy _priorityStrategy;
    private readonly ISecuritySanitizer _sanitizer; // Phase 7 Invariant Protection Hook
    private static readonly object _fileLock = new();

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        ITicketPersistenceConfiguration fileOptions,
        ITicketSystemConfiguration settings,
        ITicketPriorityStrategy priorityStrategy,
        ISecuritySanitizer sanitizer)
    {
        _ticketRepository = ticketRepository;
        _fileOptions = fileOptions;
        _settings = settings;
        _priorityStrategy = priorityStrategy;
        _sanitizer = sanitizer;
    }

    public async Task<TicketActionResponse> HandleAsync(CreateTicketCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            // Phase 7 OWASP Security Defense: Clean strings before initialization
            string safeSubject = _sanitizer.SanitizePlainString(command.Subject);
            string safeDescription = _sanitizer.SanitizePlainString(command.Description);
            string safeEmailBody = _sanitizer.SanitizeUntrustedHtml(command.EmailBody);

            var ticketEntity = OperationalTicket.OpenNewWorkspaceIncident(
                command.CustomerCode, safeSubject, safeDescription, safeEmailBody, command.EmailMessageId, _priorityStrategy
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
            await _ticketRepository.AddAsync(ticketEntity, ct);

            return new TicketActionResponse(true, ticketEntity.Id, string.Empty, ticketEntity.Priority.ToString());
        }
        catch (Exception ex)
        {
            throw new TicketCreationException(ex.Message, ex);
        }
    }
}
