using OperationalWorkspaceApplication.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace OperationalWorkspaceInfrastructure.Providers;
public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}