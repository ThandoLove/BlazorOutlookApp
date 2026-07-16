using System.IO;
using OperationalWorkspaceApplication.IServices; // FIX: Pulls your clean Application interface

namespace OperationalWorkspaceInfrastructure.Configuration;

public class TicketPersistenceOptions : ITicketPersistenceConfiguration
{
    public string DatabaseDirectory { get; set; } = @"C:\ProgramData\SageX3Workspace\Data\";
    public string DatabaseFileName { get; set; } = "workspace_tickets.json";
    public string FullDatabasePath => Path.Combine(DatabaseDirectory, DatabaseFileName);
}
