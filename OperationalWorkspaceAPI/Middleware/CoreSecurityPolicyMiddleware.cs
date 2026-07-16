using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace OperationalWorkspaceAPI.Middleware;

public class CoreSecurityPolicyMiddleware
{
    private readonly RequestDelegate _next;

    public CoreSecurityPolicyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Defend against Clickjacking by explicitly enforcing where this pane can be embedded
        context.Response.Headers.Append("X-Frame-Options", "ALLOW-FROM https://office.com https://office365.com");

        // 2. Prevent MIME type sniffing attacks
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // 3. Control cross-origin leaking vectors
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // 4. FIX: Upgraded Content Security Policy to authorize local scripts and offline styles seamlessly
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-eval' https://appsforoffice.microsoft.com; " +
            "style-src 'self' 'unsafe-inline'; " + // FIX: Grants clearance to local bootstrap and app sheets natively
            "connect-src 'self' https://workspace.local; " +
            "img-src 'self' data: https://workspace.local; " +
            "frame-ancestors 'self' https://office.com https://office365.com https://outlook.office.com;");

        await _next(context);
    }
}
