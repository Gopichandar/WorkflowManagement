using Microsoft.Extensions.DependencyInjection;
using WorkflowManagement.Core;
using WorkflowManagement.Persistence;
using WorkflowManagement.Persistence.EntityFramework.Repositories;
using WorkflowManagement.Persistence.InMemory;
using WorkflowManagement.Services;

namespace WorkflowManagement.Extensions;

public static class WorkflowServiceExtensions
{
    public static IServiceCollection AddWorkflowManagement(this IServiceCollection services, Action<WorkflowOptions> configureOptions = null)
    {
        var options = new WorkflowOptions();
        configureOptions?.Invoke(options);

        // Register core services
        services.AddScoped<IWorkflowService, WorkflowService>();

        // Register repository based on options
        if (options.UseInMemoryStorage)
        {
            services.AddSingleton<IWorkflowRepository, InMemoryWorkflowRepository>();
            services.AddSingleton<IWorkflowBlueprintRepository, InMemoryWorkflowBlueprintRepository>();
        }
        else if (options.UseSqlServer)
        {
            services.AddScoped<IWorkflowRepository, SqlServerWorkflowRepository>();
            services.AddScoped<IWorkflowBlueprintRepository, SqlServerWorkflowBlueprintRepository>();
        }

        return services;
    }
}
