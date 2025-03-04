using WorkflowManagement.Builder;
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


// GET: Retrieve all workflow instances
app.MapGet("/api/workflows", async (IWorkflowService workflowService) =>
{
var workflows = await workflowService.GetWorkflowsAsync();
return Results.Ok(workflows);
}).WithOpenApi();

// GET: Retrieve a specific workflow instance by ID
app.MapGet("/api/workflows/{id}", async (string id, IWorkflowService workflowService) =>
{
var workflow = await workflowService.GetWorkflowAsync(id);
return workflow != null ? Results.Ok(workflow) : Results.NotFound();
}).WithOpenApi();

// POST: Create a new workflow instance using an existing blueprint.
// The request requires a BlueprintId and a Name for the instance.
app.MapPost("/api/workflows", async (CreateWorkflowInstanceRequest request, IWorkflowService workflowService) =>
{
var workflow = await workflowService.CreateWorkflowInstanceAsync(
    request.BlueprintId,
    builder => builder.WithName(request.Name)
);
return Results.Created($"/api/workflows/{workflow.Id}", workflow);
}).WithOpenApi();

// POST: Advance a workflow to the next step.
app.MapPost("/api/workflows/{id}/advance", async (string id, AdvanceWorkflowRequest request, IWorkflowService workflowService) =>
{
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
catch (UnauthorizedAccessException)
{
return Results.Forbid();
}
}).WithOpenApi();

// POST: Seed a sample blueprint and create an instance using it.
app.MapPost("/api/seed", async (IWorkflowBlueprintRepository blueprintRepository, IWorkflowService workflowService) =>
{
// Create a blueprint using the new chain–style builder.
var blueprint = new WorkflowBlueprintBuilder()
    .WithName("Expense Approval Workflow")
    .WithVersion(1)
    .BeginStep("Submission")
        .AsInitial()
        .RequireRole("Employee")
        .AddExitAction("LogAction", new Dictionary<string, object>
            {
                { "message", "Expense submitted" }
            })
    .ThenStep("Manager Review")
        .RequireRole("Manager")
        .AddEntryAction("NotifyUser", new Dictionary<string, object>
            {
                { "email", "manager@company.com" },
                { "subject", "Expense needs review" }
            })
    .ThenStep("Finance Review")
        .RequireRole("Finance")
        .AddEntryAction("NotifyUser", new Dictionary<string, object>
            {
                { "email", "finance@company.com" },
                { "subject", "Expense approved by manager" }
            })
    .ThenStep("Payment Processing")
        .RequireRole("Finance")
        .AddEntryAction("ProcessPayment", new Dictionary<string, object>())
        .AddExitAction("SendEmail", new Dictionary<string, object>
            {
                { "subject", "Expense approved and paid" }
            })
    .Build();

// Save the blueprint so that it can be used to create instances.
await blueprintRepository.SaveBlueprintAsync(blueprint);

// Create a workflow instance using the newly saved blueprint.
var workflow = await workflowService.CreateWorkflowInstanceAsync(
    blueprint.Id,
    builder => builder.WithName("John's Expense Claim")
);

return Results.Ok(new
{
Message = "Sample blueprint seeded successfully",
Blueprint = blueprint,
Workflow = workflow
});
}).WithOpenApi();

// GET all blueprints
app.MapGet("/api/blueprints", async (IWorkflowBlueprintRepository blueprintRepository) =>
{
    var blueprints = await blueprintRepository.GetBlueprintsAsync();
    return Results.Ok(blueprints);
}).WithOpenApi();

// GET a specific blueprint
app.MapGet("/api/blueprints/{id}", async (string id, IWorkflowBlueprintRepository blueprintRepository) =>
{
    var blueprint = await blueprintRepository.GetBlueprintByIdAsync(id);
    return blueprint != null ? Results.Ok(blueprint) : Results.NotFound();
}).WithOpenApi();

// POST: Create a new blueprint using the chain-style builder
app.MapPost("/api/blueprints", async (CreateBlueprintRequest request, IWorkflowBlueprintRepository blueprintRepository) =>
{
    var blueprint = new WorkflowBlueprintBuilder()
        .WithName(request.Name)
        .WithVersion(request.Version)
        // Optionally, use request data to build steps dynamically
        .BeginStep("Start")
            .AsInitial()
            .RequireRole("User")
            .AddExitAction("LogAction", new Dictionary<string, object> { { "message", "Workflow started" } })
        .Build();

    await blueprintRepository.SaveBlueprintAsync(blueprint);
    return Results.Created($"/api/blueprints/{blueprint.Id}", blueprint);
}).WithOpenApi();

// PUT: Update an existing blueprint (if updates are allowed)
app.MapPut("/api/blueprints/{id}", async (string id, UpdateBlueprintRequest request, IWorkflowBlueprintRepository blueprintRepository) =>
{
    var blueprint = await blueprintRepository.GetBlueprintByIdAsync(id);
    if (blueprint == null)
        return Results.NotFound();

    // Update properties – for example, name or version. You may also rebuild steps if needed.
    blueprint.Name = request.Name;
    blueprint.Version = request.Version;
    // ... update additional fields as necessary

    await blueprintRepository.SaveBlueprintAsync(blueprint);
    return Results.Ok(blueprint);
}).WithOpenApi();

// DELETE a blueprint
app.MapDelete("/api/blueprints/{id}", async (string id, IWorkflowBlueprintRepository blueprintRepository) =>
{
    await blueprintRepository.DeleteBlueprintByIdAsync(id);
    return Results.NoContent();
}).WithOpenApi();


app.Run();


// Request DTOs for instance creation and workflow advancement

public class CreateWorkflowInstanceRequest
{
    public string BlueprintId { get; set; }
    public string Name { get; set; }
}

public class AdvanceWorkflowRequest
{
    public string UserId { get; set; }
    public string RoleId { get; set; }
    public Dictionary<string, object> Data { get; set; }
}

public class CreateBlueprintRequest
{
    /// <summary>
    /// The display name for the blueprint.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The version of the blueprint.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// The collection of steps to build this blueprint.
    /// </summary>
    public List<CreateBlueprintStepRequest> Steps { get; set; } = new();
}

public class CreateBlueprintStepRequest
{
    /// <summary>
    /// The unique name of the step.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Indicates if this step is the initial step.
    /// </summary>
    public bool IsInitial { get; set; }

    /// <summary>
    /// The roles required to execute this step.
    /// </summary>
    public List<string> RequiredRoles { get; set; } = new();

    /// <summary>
    /// Optional: The name of the next step. This is used to link steps in order.
    /// </summary>
    public string NextStepName { get; set; }

    /// <summary>
    /// A list of actions to execute when entering the step.
    /// </summary>
    public List<CreateBlueprintActionRequest> EntryActions { get; set; } = new();

    /// <summary>
    /// A list of actions to execute when exiting the step.
    /// </summary>
    public List<CreateBlueprintActionRequest> ExitActions { get; set; } = new();
}

public class CreateBlueprintActionRequest
{
    /// <summary>
    /// The name of the action.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A dictionary of parameters required by the action handler.
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class UpdateBlueprintRequest
{
    /// <summary>
    /// The updated display name for the blueprint.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The updated version number.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Optionally update the collection of steps.
    /// </summary>
    public List<UpdateBlueprintStepRequest> Steps { get; set; } = new();
}

public class UpdateBlueprintStepRequest
{
    /// <summary>
    /// The unique name of the step.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Indicates if this step should be marked as the initial step.
    /// </summary>
    public bool IsInitial { get; set; }

    /// <summary>
    /// The roles required for this step.
    /// </summary>
    public List<string> RequiredRoles { get; set; } = new();

    /// <summary>
    /// Optional: The name of the next step for linking purposes.
    /// </summary>
    public string NextStepName { get; set; }

    /// <summary>
    /// The updated list of actions to execute when entering the step.
    /// </summary>
    public List<UpdateBlueprintActionRequest> EntryActions { get; set; } = new();

    /// <summary>
    /// The updated list of actions to execute when exiting the step.
    /// </summary>
    public List<UpdateBlueprintActionRequest> ExitActions { get; set; } = new();
}

public class UpdateBlueprintActionRequest
{
    /// <summary>
    /// The updated action name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The updated dictionary of parameters for the action.
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}


// Sample action handlers (for demonstration)

public class SendEmailActionHandler
{
    public Task SendEmailAsync(string to, string subject, WorkflowContext context)
    {
        // Implement email sending logic here.
        Console.WriteLine($"EMAIL: Sending email to {to} with subject '{subject}'.");
        return Task.CompletedTask;
    }
}

public class LogActionHandler
{
    public Task LogActionAsync(string message, WorkflowContext context)
    {
        // Implement logging logic here.
        Console.WriteLine($"LOG: {message}");
        return Task.CompletedTask;
    }
}

public class NotifyUserActionHandler
{
    public Task NotifyAsync(string recipient, string message, WorkflowContext context)
    {
        // Implement notification logic here.
        Console.WriteLine($"NOTIFY: Notifying {recipient} with message '{message}'.");
        return Task.CompletedTask;
    }
}