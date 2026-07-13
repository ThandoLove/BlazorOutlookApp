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

        // 4. Tight Content Security Policy (CSP) tailored exactly for modern Office.js + Blazor script engines
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-eval' https://microsoft.com; " +
            "style-src 'self' 'unsafe-inline' https://jsdelivr.net; " +
            "connect-src 'self' https://workspace.local https://*.your-sage-x3-server.com; " +
            "img-src 'self' data: https://workspace.local; " +
            "frame-ancestors 'self' https://office.com https://office365.com;");

        await _next(context);
    }
}
