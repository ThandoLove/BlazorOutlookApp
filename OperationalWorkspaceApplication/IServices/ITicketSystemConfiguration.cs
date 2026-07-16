using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.IServices;
public interface ITicketSystemConfiguration
{
    bool UseMocks { get; }
}
