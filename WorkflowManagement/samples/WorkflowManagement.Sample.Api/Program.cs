using WorkflowManagement.Core;
using WorkflowManagement.Extensions;
using WorkflowManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddWorkflowManagement(options => {
    options.UseInMemoryStorage = true;
    // options.SqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
});

// Register workflow action handlers
builder.Services.AddTransient<SendEmailActionHandler>();
builder.Services.AddTransient<LogActionHandler>();
builder.Services.AddTransient<NotifyUserActionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/workflows", async (IWorkflowService workflowService) => {
    var workflows = await workflowService.GetWorkflowsAsync();
    return Results.Ok(workflows);
}).WithOpenApi();

app.MapGet("/api/workflows/{id}", async (string id, IWorkflowService workflowService) => {
    var workflow = await workflowService.GetWorkflowAsync(id);
    return workflow != null ? Results.Ok(workflow) : Results.NotFound();
}).WithOpenApi();

app.MapPost("/api/workflows", async (CreateWorkflowRequest request, IWorkflowService workflowService) => {
    var workflow = await workflowService.CreateWorkflowAsync(builder => {
        builder.WithName(request.Name);

        foreach (var stepRequest in request.Steps)
        {
            builder.AddStep(step => {
                step.WithName(stepRequest.Name);

                foreach (var role in stepRequest.RequiredRoles)
                {
                    step.RequireRole(role);
                }

                foreach (var actionRequest in stepRequest.Actions)
                {
                    step.AddAction(action => {
                        action.WithName(actionRequest.Name);
                        // In a real implementation, we would register handlers by name or type
                        // and resolve them dynamically here
                    });
                }
            });
        }
    });

    return Results.Created($"/api/workflows/{workflow.Id}", workflow);
}).WithOpenApi();

app.MapPost("/api/workflows/{id}/advance", async (string id, AdvanceWorkflowRequest request, IWorkflowService workflowService) => {
    try
    {
        await workflowService.MoveWorkflowToNextStepAsync(
            id,
            request.UserId,
            request.RoleId,
            request.Data ?? new Dictionary<string, object>()
        );

        return Results.Ok();
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Forbid();
    }
}).WithOpenApi();

// Initialize sample workflows
app.MapPost("/api/seed", async (IWorkflowService workflowService) => {
    // Example: Create an expense approval workflow
    await workflowService.CreateWorkflowAsync(builder => {
        builder
            .WithName("Expense Approval Workflow")
            .AddStep(step => {
                step.WithName("Submission")
                    .RequireRole("Employee")
                    .AddAction(action => {
                        action.WithName("Log Submission")
                            .WithHandler(async context => {
                                var logger = context.ServiceProvider.GetRequiredService<LogActionHandler>();
                                await logger.LogActionAsync("Expense submitted", context);
                            });
                    });
            })
            .AddStep(step => {
                step.WithName("Manager Review")
                    .RequireRole("Manager")
                    .AddAction(action => {
                        action.WithName("Notify Manager")
                            .WithHandler(async context => {
                                var notifier = context.ServiceProvider.GetRequiredService<NotifyUserActionHandler>();
                                await notifier.NotifyAsync("manager@company.com", "Expense needs review", context);
                            });
                    });
            })
            .AddStep(step => {
                step.WithName("Finance Review")
                    .RequireRole("Finance")
                    .AddAction(action => {
                        action.WithName("Notify Finance")
                            .WithHandler(async context => {
                                var notifier = context.ServiceProvider.GetRequiredService<NotifyUserActionHandler>();
                                await notifier.NotifyAsync("finance@company.com", "Expense approved by manager", context);
                            });
                    });
            })
            .AddStep(step => {
                step.WithName("Payment Processing")
                    .RequireRole("Finance")
                    .AddAction(action => {
                        action.WithName("Process Payment")
                            .WithHandler(async context => {
                                // Process payment logic here
                                await Task.CompletedTask;
                            });
                    })
                    .AddAction(action => {
                        action.WithName("Notify Employee")
                            .WithHandler(async context => {
                                var emailSender = context.ServiceProvider.GetRequiredService<SendEmailActionHandler>();
                                var employeeEmail = context.Data["EmployeeEmail"].ToString();
                                await emailSender.SendEmailAsync(employeeEmail, "Expense approved and paid", context);
                            });
                    });
            });
    });

    return Results.Ok("Sample workflows seeded successfully");
}).WithOpenApi();


app.Run();

// Define request DTOs
public class CreateWorkflowRequest
{
    public string Name { get; set; }
    public List<CreateWorkflowStepRequest> Steps { get; set; } = new();
}

public class CreateWorkflowStepRequest
{
    public string Name { get; set; }
    public List<string> RequiredRoles { get; set; } = new();
    public List<CreateWorkflowActionRequest> Actions { get; set; } = new();
}

public class CreateWorkflowActionRequest
{
    public string Name { get; set; }
    public string HandlerType { get; set; }
}

public class AdvanceWorkflowRequest
{
    public string UserId { get; set; }
    public string RoleId { get; set; }
    public Dictionary<string, object> Data { get; set; }
}

// Sample action handlers
public class SendEmailActionHandler
{
    public Task SendEmailAsync(string to, string subject, WorkflowContext context)
    {
        // Implement email sending logic
        return Task.CompletedTask;
    }
}

public class LogActionHandler
{
    public Task LogActionAsync(string message, WorkflowContext context)
    {
        // Implement logging logic
        return Task.CompletedTask;
    }
}

public class NotifyUserActionHandler
{
    public Task NotifyAsync(string userId, string message, WorkflowContext context)
    {
        // Implement notification logic
        return Task.CompletedTask;
    }
}
