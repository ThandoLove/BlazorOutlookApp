using OperationalWorkspaceDomain.Enums.TicketsEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceDomain.Strategies;

public interface ITicketPriorityStrategy
{
    TicketPriority DeterminePriority(string subject, string description, string emailBody);
}
