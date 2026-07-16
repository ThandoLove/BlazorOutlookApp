using OperationalWorkspace.Infrastructure.InfraExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspace.Infrastructure.InfraExceptions;
public class AuthenticationException : InfrastructureException
{
    public AuthenticationException(string message) : base($"[Auth Failure] {message}") { }
    public AuthenticationException(string message, Exception inner) : base($"[Auth Failure] {message}", inner) { }
}