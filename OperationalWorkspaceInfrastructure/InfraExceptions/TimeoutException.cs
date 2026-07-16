using OperationalWorkspace.Infrastructure.InfraExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspace.Infrastructure.InfraExceptions;
public class TimeoutException : InfrastructureException
{
    public TimeoutException(string operation, int timeoutSeconds)
        : base($"[Timeout Error] Operation '{operation}' exceeded allocated execution budget of {timeoutSeconds}s.") { }
}
