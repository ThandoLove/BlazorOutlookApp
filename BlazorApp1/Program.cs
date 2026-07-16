using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OperationalWorkspaceUI.Components;
using OperationalWorkspaceUI.UIState;
using OperationalWorkspaceUI.UIServices;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// 1. Register shared Flux-style state machines natively across execution cycles
builder.Services.AddScoped<UIStateContainer>();

// 2. REPAIR BLOCK: Explicitly pass logger dependencies inside the Typed Client Factory mapping rules
builder.Services.AddHttpClient<IWorkspaceApiService, WorkspaceApiService>((serviceProvider, client) =>
{
    // If you need to seed the base address from environmental configuration flags:
    // client.BaseAddress = new Uri(builder.Configuration["SageX3Settings:BaseUrl"] ?? "https://workspace.local");
})
.AddTypedClient<IWorkspaceApiService>((httpClient, serviceProvider) =>
{
    var state = serviceProvider.GetRequiredService<UIStateContainer>();
    var logger = serviceProvider.GetRequiredService<ILogger<WorkspaceApiService>>();

    return new WorkspaceApiService(httpClient, state, logger);
});

// 3. Add core Razor component rendering services to the container
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
