using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OperationalWorkspaceAPI.Extensions;
using OperationalWorkspaceAPI.Middleware;
using OperationalWorkspaceApplication.Abstractions;
using OperationalWorkspaceInfrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.RegisterCoreWorkspaceDependencies(builder.Configuration);

// Phase 7 Audit: Wire your security sanitizer instance into the composition container root
builder.Services.AddSingleton<ISecuritySanitizer, ProductionSecuritySanitizer>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Section 10 CORS Resolution: Explicitly restrict origins to mitigate scripting forgery vectors
builder.Services.AddCors(options =>
{
    options.AddPolicy("OutlookWorkspaceCorsPolicy", policy =>
    {
        policy.WithOrigins("https://workspace.local") // Only trust your specific, secure add-in host
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Cache-Status"); // Expose only required telemetry metrics
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Section 15 HTTPS & Transport Hardening: Force explicit secure encryption limits on server nodes
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<CoreSecurityPolicyMiddleware>();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("OutlookWorkspaceCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
