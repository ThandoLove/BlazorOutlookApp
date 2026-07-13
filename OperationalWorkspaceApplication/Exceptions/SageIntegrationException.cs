using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Exceptions;
public class SageIntegrationException : Exception
{
    public string ErrorCodeCode { get; }
    public bool IsTransientFault { get; }

    public SageIntegrationException(string message, string errorCode, bool isTransient, Exception? innerException = null)
        : base($"{message} (System Fault Code: {errorCode})", innerException)
    {
        ErrorCodeCode = errorCode;
        IsTransientFault = isTransient;
    }
}
