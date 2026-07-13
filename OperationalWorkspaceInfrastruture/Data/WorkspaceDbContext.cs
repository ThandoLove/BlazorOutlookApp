using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OperationalWorkspaceDomain.Entities;

namespace OperationalWorkspaceInfrastruture.Data;


public class WorkspaceDbContext : DbContext
{
    public WorkspaceDbContext(DbContextOptions<WorkspaceDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SageDocument> Documents => Set<SageDocument>();
    public DbSet<OperationalTicket> Tickets => Set<OperationalTicket>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(20);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<SageDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentNumber);
            entity.Property(e => e.DocumentNumber).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(30);
            entity.Property(e => e.Status).HasMaxLength(30);
        });

        modelBuilder.Entity<OperationalTicket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Priority).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.HasIndex(e => e.CustomerId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(150);
            entity.Property(e => e.ActionType).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.UserEmail);
            entity.HasIndex(e => e.TimestampUtc);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Enforce systemic validation checks or interceptors before updating storage frames
        return base.SaveChangesAsync(cancellationToken);
    }
}
