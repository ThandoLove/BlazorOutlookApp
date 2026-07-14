using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OperationalWorkspaceAPI.Extensions;
using OperationalWorkspaceAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- MANDATORY INFRASTRUCTURE REPAIR LINE ---
// Explicitly registers IHttpContextAccessor so your custom Authorization policies can safely extract request headers
builder.Services.AddHttpContextAccessor();

// 1. Centralize Dependency Injection assemblies across projects
builder.Services.RegisterCoreWorkspaceDependencies(builder.Configuration);

// 2. Add Blazor Unified Server-Side Component Rendering engines
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS specifically matching your local secure DNS address bindings
builder.Services.AddCors(options =>
{
    options.AddPolicy("OutlookWorkspaceCorsPolicy", policy =>
    {
        policy.WithOrigins("https://workspace.local")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 3. Inject strict security headers required by modern Outlook manifests
app.UseMiddleware<CoreSecurityPolicyMiddleware>();

// 4. Enable static file delivery to host UI styles, images, and the manifest file
app.UseStaticFiles();

app.UseRouting();
app.UseCors("OutlookWorkspaceCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery(); // Enforces anti-forgery protection across unified Blazor forms

// 5. Unified routing endpoints map
app.MapControllers();
app.MapBlazorHub(); // Hooks up the real-time pipeline connection for Blazor
app.MapFallbackToPage("/_Host"); // Redirects non-API traffic smoothly into the UI Hub viewports

app.Run();
