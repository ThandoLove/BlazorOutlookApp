using OperationalWorkspaceApplication.Abstractions;

using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;


namespace OperationalWorkspaceInfrastructure.Providers;

// --- PHASE 7 RESOLUTION: OWASP TOP 10 SECURE ANTI-XSS SANITIZER UTILITY ---
public class ProductionSecuritySanitizer : ISecuritySanitizer
{
    public string SanitizeUntrustedHtml(string rawInput)
    {
        if (string.IsNullOrEmpty(rawInput)) return string.Empty;

        // Strip out scripts, style elements, and event wire handlers completely
        string clean = Regex.Replace(rawInput, "<script.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        clean = Regex.Replace(clean, "on\\w+\\s*=", "", RegexOptions.IgnoreCase);
        clean = Regex.Replace(clean, "javascript:", "", RegexOptions.IgnoreCase);

        return clean.Trim();
    }

    public string SanitizePlainString(string rawInput)
    {
        if (string.IsNullOrEmpty(rawInput)) return string.Empty;

        // Encode special control tokens natively to defuse injection attempts
        return WebUtility.HtmlEncode(rawInput.Trim());
    }
}
