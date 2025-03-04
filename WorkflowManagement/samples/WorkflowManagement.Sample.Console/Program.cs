using Microsoft.Extensions.DependencyInjection;
using WorkflowManagement.Builder;
using WorkflowManagement.Core;
using WorkflowManagement.Core.Models;
using WorkflowManagement.Extensions;
using WorkflowManagement.Persistence.InMemory;
using WorkflowManagement.Persistence;
using WorkflowManagement.Services;

// Setup dependency injection
var serviceProvider = ConfigureServices();

// Get workflow service
var workflowService = serviceProvider.GetRequiredService<IWorkflowService>();
var blueprintRepository = serviceProvider.GetRequiredService<IWorkflowBlueprintRepository>();

Console.WriteLine("Workflow Management Console");
Console.WriteLine("==========================");

// Create and save a workflow blueprint
var expenseApprovalBlueprint = await CreateExpenseApprovalBlueprintAsync(blueprintRepository);
Console.WriteLine($"Created blueprint: {expenseApprovalBlueprint.Name} (ID: {expenseApprovalBlueprint.Id})");

// Create a workflow instance using the blueprint
var workflow = await workflowService.CreateWorkflowInstanceAsync(
    expenseApprovalBlueprint.Id,
    builder => builder.WithName("John's Expense Claim")
);

Console.WriteLine($"Created workflow: {workflow.Name} (ID: {workflow.Id})");
Console.WriteLine($"Current step: {workflow.CurrentStep.Name}");

// Simulate workflow progression
await SimulateWorkflowProgressionAsync(workflowService, workflow);

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

static IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();

    // Register repositories
    services.AddWorkflowManagement(x => x.UseInMemoryStorage = true);

    // Register action handlers
    services.AddTransient<LogActionHandler>();
    services.AddTransient<NotifyUserActionHandler>();
    services.AddTransient<SendEmailActionHandler>();

    return services.BuildServiceProvider();
}
static async Task<WorkflowBlueprint> CreateExpenseApprovalBlueprintAsync(IWorkflowBlueprintRepository blueprintRepository)
{
    // Create a workflow using a fluent chain-style builder
    var blueprint = new WorkflowBlueprintBuilder()
        .WithName("Expense Approval Workflow")
        .WithVersion(1)
        .BeginStep("Submission")
            .AsInitial()
            .RequireRole("Employee")
            .AddExitAction("LogAction", new Dictionary<string, object> { { "message", "Expense submitted" } })
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
            .AddExitAction("SendEmail", new Dictionary<string, object> { { "subject", "Expense approved and paid" } })
        .Build();

    await blueprintRepository.SaveBlueprintAsync(blueprint);
    return blueprint;
}

static async Task SimulateWorkflowProgressionAsync(IWorkflowService workflowService, IWorkflow workflow)
{
    Console.WriteLine("\nSimulating workflow progression:");

    // 1. Employee submits expense
    Console.WriteLine("\nStep 1: Employee submits expense");
    var submissionData = new Dictionary<string, object>
            {
                { "amount", 1250.00 },
                { "description", "Conference travel expenses" },
                { "EmployeeEmail", "john@company.com" }
            };

    await workflowService.MoveWorkflowToNextStepAsync(
        workflow.Id,
        "user-1",
        "Employee",
        submissionData);

    workflow = await workflowService.GetWorkflowAsync(workflow.Id);
    Console.WriteLine($"Current step: {workflow.CurrentStep.Name}");

    // 2. Manager reviews and approves
    Console.WriteLine("\nStep 2: Manager reviews and approves");
    var managerData = new Dictionary<string, object>
            {
                { "approved", true },
                { "comments", "Approved as per policy" }
            };

    await workflowService.MoveWorkflowToNextStepAsync(
        workflow.Id,
        "user-2",
        "Manager",
        managerData);

    workflow = await workflowService.GetWorkflowAsync(workflow.Id);
    Console.WriteLine($"Current step: {workflow.CurrentStep.Name}");

    // 3. Finance reviews and processes
    Console.WriteLine("\nStep 3: Finance reviews and processes");
    var financeData = new Dictionary<string, object>
            {
                { "costCenter", "DEPT-001" },
                { "paymentMethod", "BankTransfer" }
            };

    await workflowService.MoveWorkflowToNextStepAsync(
        workflow.Id,
        "user-3",
        "Finance",
        financeData);

    workflow = await workflowService.GetWorkflowAsync(workflow.Id);
    Console.WriteLine($"Current step: {workflow.CurrentStep.Name}");

    // 4. Finance completes payment
    Console.WriteLine("\nStep 4: Finance completes payment");
    var paymentData = new Dictionary<string, object>
            {
                { "paymentReference", "PAY-2025-0001" },
                { "paymentDate", DateTime.Now }
            };

    await workflowService.MoveWorkflowToNextStepAsync(
        workflow.Id,
        "user-3",
        "Finance",
        paymentData);

    workflow = await workflowService.GetWorkflowAsync(workflow.Id);
    Console.WriteLine($"Workflow completed: {workflow.IsCompleted}");
}

// Sample action handlers
public class LogActionHandler
{
    public Task LogAsync(string message)
    {
        System.Console.WriteLine($"LOG: {message}");
        return Task.CompletedTask;
    }
}
    
public class NotifyUserActionHandler
{
    public Task NotifyAsync(string recipient, string message)
    {
        System.Console.WriteLine($"NOTIFICATION to {recipient}: {message}");
        return Task.CompletedTask;
    }
}


public class SendEmailActionHandler
{
    public Task SendEmailAsync(string to, string subject, WorkflowContext context)
    {
        Console.WriteLine($"EMAIL to {to}: {subject}");
        return Task.CompletedTask;
    }
}