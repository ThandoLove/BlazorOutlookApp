using Microsoft.EntityFrameworkCore;
using OperationalWorkspaceDomain.Entities;
using OperationalWorkspaceDomain.Enums;
using OperationalWorkspaceDomain.Enums.TicketsEnum;

namespace OperationalWorkspaceInfrastructure.Data;

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

        // ... Keep your existing Customer, SageDocument, and AuditLog mapping rules completely identical ...

        modelBuilder.Entity<OperationalTicket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(20);

            // FIX: Tells EF Core to automatically map C# Rich Enums into plain text strings inside SQL tables
            entity.Property(e => e.Priority)
                  .HasConversion(v => v.ToString(), v => (TicketPriority)System.Enum.Parse(typeof(TicketPriority), v))
                  .HasMaxLength(20);

            entity.Property(e => e.Status)
                  .HasConversion(v => v.ToString(), v => (TicketStatus)System.Enum.Parse(typeof(TicketStatus), v))
                  .HasMaxLength(20);

            entity.HasIndex(e => e.CustomerId);
        });
    }
}
