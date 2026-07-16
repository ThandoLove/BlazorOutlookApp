using OperationalWorkspace.Infrastructure.InfraExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspace.Infrastructure.InfraExceptions;
public class NetworkException : InfrastructureException
{
    public NetworkException(string operation, string endpoint, Exception inner)
        : base($"[Network Error] Operation '{operation}' failed on endpoint '{endpoint}'.", inner) { }
}