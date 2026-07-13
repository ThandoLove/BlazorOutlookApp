using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OperationalWorkspaceInfrastruture.Configuration;


public class TicketPersistenceOptions
{
    public string DatabaseDirectory { get; set; } = @"C:\ProgramData\SageX3Workspace\Data\";
    public string DatabaseFileName { get; set; } = "workspace_tickets.json";
    public string FullDatabasePath => Path.Combine(DatabaseDirectory, DatabaseFileName);
}
