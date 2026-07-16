using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace OperationalWorkspaceDomain.Events;

public record CustomerResolvedEvent(string BpCode, string CompanyName, string ResolvedByEmail, DateTime Timestamp);
public record TicketCreatedEvent(string TicketId, string CustomerId, string AssignedPriority, DateTime Timestamp);
public record DocumentViewedEvent(string DocumentNumber, string BpCode, string ActionedByEmail, DateTime Timestamp);
public record EmailProcessedEvent(string MessageId, string SenderEmail, string HandledByEmail, DateTime Timestamp);
public record AttachmentDownloadedEvent(string BlobKey, string DocumentNumber, string ExtractedByEmail, DateTime Timestamp);
