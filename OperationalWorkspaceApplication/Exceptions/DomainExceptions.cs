using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Exceptions;


public abstract class WorkspaceException : Exception
{
    protected WorkspaceException(string message) : base(message) { }
    protected WorkspaceException(string message, Exception innerException) : base(message, innerException) { }
}

public class CustomerNotFoundException : WorkspaceException
{
    public CustomerNotFoundException(string bpCode)
        : base($"Registered matching Sage X3 account '{bpCode}' could not be resolved inside requested data structures.") { }
}

public class TicketCreationException : WorkspaceException
{
    public TicketCreationException(string message, Exception inner)
        : base($"Failed to open new operational incident ticket track: {message}", inner) { }
}

public class SageAuthenticationException : WorkspaceException
{
    public SageAuthenticationException(string reason)
        : base($"Syracuse Identity Provider handshake failure: {reason}") { }
}

public class GraphQLQueryException : WorkspaceException
{
    public GraphQLQueryException(string operation, string errorRaw)
        : base($"Sage X3 GraphQL node parsing failure during execution of '{operation}'. Details: {errorRaw}") { }
}

public class DocumentAccessException : WorkspaceException
{
    public DocumentAccessException(string docNumber, string reason)
        : base($"Secure binary stream mapping or permission evaluation rejected access to file '{docNumber}'. Reason: {reason}") { }
}