using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspace.Infrastructure.InfraExceptions;

public abstract class InfrastructureException : Exception
{
    protected InfrastructureException(string message) : base(message) { }
    protected InfrastructureException(string message, Exception inner) : base(message, inner) { }
}