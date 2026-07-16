using OperationalWorkspaceApplication.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace OperationalWorkspaceInfrastructure.Providers;

public class OutlookEmailContextProvider : IEmailContextProvider
{
    public string ExtractMessageIdFromHeader(IDictionary<string, string> headers)
    {
        if (headers.TryGetValue("Message-ID", out var id)) return id;
        return $"GEN-OUTLOOK-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
    }

    public string SanitizeEmailBodyPayload(string rawHtmlBody)
    {
        if (string.IsNullOrEmpty(rawHtmlBody)) return string.Empty;
        // Strip out volatile web scripts or tracking tokens without exposing or mutating raw user fields
        return Regex.Replace(rawHtmlBody, "<script.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline).Trim();
    }
}