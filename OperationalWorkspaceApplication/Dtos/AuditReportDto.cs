
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Dtos;


public record AuditReportDto(int TotalTicketsFiled, int OpenCount, int HighPriorityCount, DateTime GeneratedAt, List<TicketDto> TicketLogs);


