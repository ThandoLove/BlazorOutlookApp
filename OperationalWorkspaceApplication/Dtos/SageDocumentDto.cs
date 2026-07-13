using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Dtos;


public record SageDocumentDto(
    string DocumentNumber,
    string Type,
    DateTime Date,
    decimal Amount,
    string Currency,
    string Status,
    string MimeType,
    string InternalSageBlobId,
    bool IsActionableFinancialItem
);

