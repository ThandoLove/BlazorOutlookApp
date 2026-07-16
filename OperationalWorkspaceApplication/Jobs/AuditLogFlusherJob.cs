using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OperationalWorkspaceApplication.AppData; // Abstraction reference layer
using OperationalWorkspaceDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Jobs;

public interface IAuditLogQueue
{
    void QueueAuditEntry(AuditLog entry);
}

public class AuditLogFlusherJob : BackgroundService, IAuditLogQueue
{
    private readonly Channel<AuditLog> _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditLogFlusherJob> _logger;

    public AuditLogFlusherJob(IServiceProvider serviceProvider, ILogger<AuditLogFlusherJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var options = new BoundedChannelOptions(5000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<AuditLog>(options);
    }

    public void QueueAuditEntry(AuditLog entry)
    {
        if (!_channel.Writer.TryWrite(entry))
        {
            _logger.LogWarning("Audit event buffer pipeline is saturated. Discarding older tracing telemetry rows.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Asynchronous Security Audit Log background worker thread initialized successfully.");

        while (await _channel.Reader.WaitToReadAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                // FIX: Request the clean data store interface, completely hiding EF Core from the Application Layer
                var dataStore = scope.ServiceProvider.GetRequiredService<IWorkspaceDataStore>();
                var batch = new List<AuditLog>();

                while (_channel.Reader.TryRead(out var logEntry))
                {
                    batch.Add(logEntry);
                }

                if (batch.Count > 0)
                {
                    // FIX: Save logs through the abstraction layer boundary smoothly
                    await dataStore.AddAuditLogBatchAsync(batch, stoppingToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Background pipeline worker failed writing accumulated compliance records back to SQL Server.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
