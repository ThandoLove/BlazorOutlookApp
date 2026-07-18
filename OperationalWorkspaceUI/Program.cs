using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OperationalWorkspaceUI.Components;
using OperationalWorkspaceUI.UIState;
using OperationalWorkspaceUI.UIServices;
using System;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// 1. Register the shared session-scoped state machine natively across execution cycles
builder.Services.AddScoped<UIStateContainer>();

// 2. PHASE 5 RESOLUTION: Strict Production Mock Authentication Environment Guard
bool useMocks = builder.Configuration.GetValue<bool>("SageX3Settings:UseMocks");
bool useMockAuth = builder.Configuration.GetValue<bool>("SageX3Settings:UseMockAuth");

if ((useMocks || useMockAuth) && !builder.Environment.IsDevelopment())
{
    // Hard crash on startup to prevent catastrophic data leakage or mock authentication vulnerabilities in production
    throw new InvalidOperationException(
        "CRITICAL SECURITY ALERT: Staged Mock Data Modes or Simulated Mock Authentication vectors " +
        $"cannot be enabled within a production environment context ({builder.Environment.EnvironmentName}). " +
        "Please check appsettings.json configuration keys immediately.");
}

// 3. Simplified Typed HttpClient Factory Registration
builder.Services.AddHttpClient<IWorkspaceApiService, WorkspaceApiService>((serviceProvider, client) =>
{
    // Configure base endpoint routing parameters straight from local configuration keys if needed
    // client.BaseAddress = new Uri(builder.Configuration["SageX3Settings:BaseUrl"] ?? "https://workspace.local");
});

// 4. Add core Razor component rendering services to the host container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
