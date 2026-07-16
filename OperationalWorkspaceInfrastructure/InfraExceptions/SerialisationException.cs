using OperationalWorkspace.Infrastructure.InfraExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspace.Infrastructure.InfraExceptions;
public class SerializationException : InfrastructureException
{
    public SerializationException(string message, Exception inner) : base($"[Serialization Stream Mismatch] {message}", inner) { }
}