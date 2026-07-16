using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OperationalWorkspaceAPI.Policies;
using OperationalWorkspaceApplication.AppData;
using OperationalWorkspaceApplication.AppSecurity;
using OperationalWorkspaceApplication.IServices;
using OperationalWorkspaceApplication.Jobs;
using OperationalWorkspaceApplication.Services;
using OperationalWorkspaceInfrastructure.Configuration;
using OperationalWorkspaceInfrastructure.Data;
using OperationalWorkspaceInfrastructure.InfraServices;
using OperationalWorkspaceInfrastructure.SecurityInfra;
using OperationalWorkspaceApplication.Abstractions;
using OperationalWorkspaceInfrastructure.Providers;
using OperationalWorkspaceInfrastructure.SageX3.X3Services;
using OperationalWorkspaceApplication.IRepositories;
using OperationalWorkspaceInfrastructure.Repositories;
using OperationalWorkspaceAPI.Logging;
using System;

namespace OperationalWorkspaceAPI.Extensions;

public static class ApiDependencyInjectionExtensions
{
    public static IServiceCollection RegisterCoreWorkspaceDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Bind appsettings configuration blocks straight to options classes
        services.Configure<SageX3Settings>(configuration.GetSection("SageX3Settings"));
        services.Configure<SageOAuthConfig>(configuration.GetSection("SageOAuthConfig"));
        services.Configure<TicketPersistenceOptions>(configuration.GetSection("TicketPersistenceOptions"));

        // 2. Concrete Cryptography Strategy Injection
        services.AddSingleton<ITokenEncryptionProvider, TokenEncryptionProvider>();
        services.AddSingleton<OperationalWorkspaceDomain.Strategies.ITicketPriorityStrategy, OperationalWorkspaceDomain.Strategies.KeywordPriorityStrategy>();
        services.AddMemoryCache();

        // 3. Resolve the Dependency Inversion bridges to inject configurations into Application use-cases
        services.AddSingleton<ISageX3Configuration>(sp =>
            sp.GetRequiredService<IOptions<SageX3Settings>>().Value);

        services.AddSingleton<ITicketPersistenceConfiguration>(sp =>
            sp.GetRequiredService<IOptions<TicketPersistenceOptions>>().Value);

        services.AddSingleton<ITicketSystemConfiguration>(sp =>
            sp.GetRequiredService<IOptions<SageX3Settings>>().Value);

        // 4. Phase 3 Audit: Register Extracted Enterprise Subsystem Providers
        services.AddSingleton<ITokenCache, MemoryTokenCache>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<IAuditLogger, ProductionAuditLogger>();
        services.AddSingleton<ICorrelationIdProvider, SequentialCorrelationIdProvider>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IEmailContextProvider, OutlookEmailContextProvider>();
        // --- REGISTER ENTERPRISE RUNTIME METRICS TELEMETRY TRACKER ---
        services.AddSingleton<ITelemetryProcessor, EnterpriseTelemetryProcessor>();


        // Phase 7 Audit: Wire your security sanitizer instance into the composition container root
        services.AddSingleton<ISecuritySanitizer, ProductionSecuritySanitizer>();

        // 5. Ingest environment parameters and build connection strings
        var baseConnString = configuration.GetConnectionString("DefaultConnection");
        var dbUser = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection__User", EnvironmentVariableTarget.Machine);
        var dbPass = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection__Password", EnvironmentVariableTarget.Machine);
        var finalizedString = $"{baseConnString}User ID={dbUser};Password={dbPass};";

        services.AddDbContext<WorkspaceDbContext>(options =>
            options.UseSqlServer(finalizedString, sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

        // 6. Map concrete repository drivers straight to Application storage boundary interfaces
        services.AddScoped<IWorkspaceDataStore, WorkspaceDataStore>();

        // 7. Bind HTTP Clients
        services.AddHttpClient<ISageAuthService, SageAuthService>(client =>
        {
            client.BaseAddress = new Uri(configuration["SageX3Settings:BaseUrl"] ?? "https://workspace.local");
        });
        services.AddHttpClient<ISageX3GraphQLClient, SageX3GraphQLClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["SageX3Settings:BaseUrl"] ?? "https://workspace.local");
        });

        // Phase 9 Audit: Register clean, unified entry-point infrastructure integration gateway components
        services.AddHttpClient<ISageX3Gateway, SageX3Gateway>(client =>
        {
            client.BaseAddress = new Uri(configuration["SageX3Settings:BaseUrl"] ?? "https://workspace.local");
        });

        // 8. Inject use-case orchestration service boundaries
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddSingleton<IPremiumWorkspaceEngine, PremiumWorkspaceEngine>();

        // --- REGISTER SPECIALIZED CQRS HANDLERS ---
        services.AddScoped<OperationalWorkspaceApplication.Handlers.CreateTicketCommandHandler>();
        services.AddScoped<OperationalWorkspaceApplication.Handlers.GetCustomerContextQueryHandler>();

        // --- REGISTER STRATEGIC REPOSITORY CONTRACTS ---
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        // 9. Wire high-performance asynchronous background log flusher jobs
        services.AddSingleton<AuditLogFlusherJob>();
        services.AddHostedService(sp => sp.GetRequiredService<AuditLogFlusherJob>());
        services.AddSingleton<IAuditLogQueue>(sp => sp.GetRequiredService<AuditLogFlusherJob>());

        // 10. Enforce system security parameters and attach explicit web route-protection policies
        services.AddTransient<IAuthorizationHandler, AuthorizationPolicyHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("LedgerReadAccess", policy => policy.Requirements.Add(new SageRoleRequirement("Admin;Finance;Accounting")));
            options.AddPolicy("PipelineWriteAccess", policy => policy.Requirements.Add(new SageRoleRequirement("Admin;Sales;Consultant")));
            options.AddPolicy("RegistryAdminAccess", policy => policy.Requirements.Add(new SageRoleRequirement("Admin")));
        });

        // --- SAGE X3 ENTERPRISE INTEGRATION DIAGNOSTICS REGISTRY ---
        // Bind an isolated HttpClient specifically for the health check network probes
        services.AddHttpClient<OperationalWorkspaceInfrastructure.SageX3.X3Diagnostics.SageX3HealthCheck>(client =>
        {
            client.BaseAddress = new Uri(configuration["SageX3Settings:BaseUrl"] ?? "https://workspace.local");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        // Register your custom infrastructure probe inside the native Microsoft health engine container
        services.AddHealthChecks()
            .AddCheck<OperationalWorkspaceInfrastructure.SageX3.X3Diagnostics.SageX3HealthCheck>("SageX3_Endpoint_Check");

        return services;
    }
}
