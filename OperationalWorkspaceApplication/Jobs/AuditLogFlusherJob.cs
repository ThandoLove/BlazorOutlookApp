using OperationalWorkspaceDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

using System.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OperationalWorkspaceInfrastruture.Data;

namespace OperationalWorkspaceApplication.Jobs;

public interface IAuditLogQueue
{
    void QueueAuditEntry(AuditLog entry);
}

public class AuditLogFlusherJob : BackgroundService, IAuditLogQueue
{
    // High-performance Channel to execute thread-safe non-blocking async pipeline buffers
    private readonly Channel<AuditLog> _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditLogFlusherJob> _logger;

    public AuditLogFlusherJob(IServiceProvider serviceProvider, ILogger<AuditLogFlusherJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Constrain bounding targets to safely mitigate out-of-memory memory explosions under peak operations
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

        // Continuous processing background parsing sequence loops
        while (await _channel.Reader.WaitToReadAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkspaceDbContext>();

                // Process internal channel frames concurrently while data strings exist
                while (_channel.Reader.TryRead(out var logEntry))
                {
                    await dbContext.AuditLogs.AddAsync(logEntry, stoppingToken);
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Background pipeline worker failed writing accumulated compliance records back to SQL Server.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Circuit safety window cooling delay
            }
        }
    }
}
