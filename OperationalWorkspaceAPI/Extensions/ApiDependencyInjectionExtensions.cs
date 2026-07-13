using Microsoft.AspNetCore.Authorization;
using OperationalWorkspaceAPI.Policies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace OperationalWorkspaceAPI.Extensions;


public static class ApiDependencyInjectionExtensions
{
    public static IServiceCollection RegisterCoreWorkspaceDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // ... (Keep existing registrations for Infrastructure, Database and Jobs here) ...

        // 5. Clean, Professional Authorization Policies Setup (Standardized Conventions)
        services.AddTransient<IAuthorizationHandler, AuthorizationPolicyHandler>();
        services.AddAuthorization(options =>
        {
            // Renamed away from narrow department labels to standardized, enterprise action-based policies
            options.AddPolicy("LedgerReadAccess", policy => policy.Requirements.Add(new SageRoleRequirement("Admin;Finance;Accounting")));
            options.AddPolicy("PipelineWriteAccess", policy => policy.Requirements.Add(new SageRoleRequirement("Admin;Sales;Consultant")));
            options.AddPolicy("RegistryAdminAccess", policy => policy.Requirements.Add(new SageRoleRequirement("Admin")));
        });

        return services;
    }
}
