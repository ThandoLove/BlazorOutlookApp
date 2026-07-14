using OperationalWorkspaceUI.Components;
using OperationalWorkspaceUI.UIState;
using OperationalWorkspaceUI.UIServices;
using System;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// --- WORKSPACE UI SERVICE REGISTRATIONS ---

// 1. Register the core .NET HttpClient infrastructure framework
builder.Services.AddHttpClient();

// 2. Register your Flux-style in-memory data store across server-side execution cycles
builder.Services.AddScoped<UIStateContainer>();

// 3. Register your client API communication proxy channel with an explicit constructor factory wrapper
builder.Services.AddScoped<IWorkspaceApiService>(serviceProvider =>
{
    var httpClient = serviceProvider.GetRequiredService<HttpClient>();
    var stateContainer = serviceProvider.GetRequiredService<UIStateContainer>();

    // Explicitly seed the target base address if it hasn't been set by host environment flags
    if (httpClient.BaseAddress == null)
    {
        httpClient.BaseAddress = new Uri("https://workspace.local");
    }

    return new WorkspaceApiService(httpClient, stateContainer);
});

// Add core Razor component rendering services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Map and bind the interactive server component tree definitions
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
