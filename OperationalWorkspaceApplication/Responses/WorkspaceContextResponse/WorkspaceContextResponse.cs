using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OperationalWorkspaceApplication.Dtos;

namespace OperationalWorkspaceApplication.Responses.WorkspaceContextResponse;


public record WorkspaceContextResponse(
    CustomerDto? Customer,
    List<SageDocumentDto> RecentDocuments,
    string AiExecutiveBrief,
    bool IsInternalEmployee,
    string SuggestedNextActionCard,
    string RecommendedActionStyleMarker // e.g. "Danger", "Success", "Warning"
);
