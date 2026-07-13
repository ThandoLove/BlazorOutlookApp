using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Dtos;


public record TicketDto(string Id, string CustomerId, string Subject, string Description, string Priority, string Status, DateTime CreatedAt);
