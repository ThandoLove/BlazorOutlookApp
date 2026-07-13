using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Responses.TicketResponse;


public record TicketActionResponse(bool IsSuccess, string TicketId, string ValidationErrorMessage, string AssignedPriority);
