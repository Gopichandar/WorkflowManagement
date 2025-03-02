using Microsoft.Extensions.DependencyInjection;
using WorkflowManagement.Core;
using WorkflowManagement.Extensions;
using WorkflowManagement.Services;

System.Console.WriteLine("Workflow Management Library Demo");
System.Console.WriteLine("================================\n");

// Setup dependency injection
var services = new ServiceCollection();

// Use in-memory storage for simplicity
services.AddWorkflowManagement(options => {
    options.UseInMemoryStorage = true;
});

// Register action handlers
services.AddTransient<LogActionHandler>();
services.AddTransient<NotificationActionHandler>();

var serviceProvider = services.BuildServiceProvider();
var workflowService = serviceProvider.GetRequiredService<IWorkflowService>();

// Create a sample workflow
System.Console.WriteLine("Creating sample workflow...");
var workflow = await CreateSampleWorkflowAsync(workflowService);
System.Console.WriteLine($"Created workflow: {workflow.Name} (ID: {workflow.Id})");

// Display workflow details
DisplayWorkflow(workflow);

// Process the workflow through its steps
await ProcessWorkflowAsync(workflow, workflowService);

System.Console.WriteLine("\nDemo completed. Press any key to exit.");
System.Console.ReadKey();


static async Task<IWorkflow> CreateSampleWorkflowAsync(IWorkflowService workflowService)
{
    return await workflowService.CreateWorkflowAsync(builder => {
        builder
            .WithName("Document Approval Process")
            .AddStep(step => {
                step.WithName("Draft")
                    .RequireRole("Author")
                    .AddAction(action => {
                        action.WithName("Create Document")
                            .WithHandler(async context => {
                                System.Console.WriteLine("Action: Document drafted");
                                context.Data["DocumentId"] = Guid.NewGuid().ToString();
                                context.Data["DocumentTitle"] = "Sample Document";
                                context.Data["Content"] = "This is sample content";
                                await Task.CompletedTask;
                            });
                    });
            })
            .AddStep(step => {
                step.WithName("Review")
                    .RequireRole("Reviewer")
                    .AddAction(action => {
                        action.WithName("Review Document")
                            .WithHandler(async context => {
                                System.Console.WriteLine("Action: Document reviewed");
                                context.Data["ReviewedBy"] = "John Reviewer";
                                context.Data["ReviewDate"] = DateTime.Now;
                                context.Data["Comments"] = "Looks good!";
                                await Task.CompletedTask;
                            });
                    });
            })
            .AddStep(step => {
                step.WithName("Approve")
                    .RequireRole("Manager")
                    .AddAction(action => {
                        action.WithName("Approve Document")
                            .WithHandler(async context => {
                                System.Console.WriteLine("Action: Document approved");
                                context.Data["ApprovedBy"] = "Jane Manager";
                                context.Data["ApprovalDate"] = DateTime.Now;
                                await Task.CompletedTask;
                            });
                    });
            })
            .AddStep(step => {
                step.WithName("Publish")
                    .RequireRole("Publisher")
                    .AddAction(action => {
                        action.WithName("Publish Document")
                            .WithHandler(async context => {
                                System.Console.WriteLine("Action: Document published");
                                context.Data["PublishedBy"] = "Bob Publisher";
                                context.Data["PublishDate"] = DateTime.Now;
                                context.Data["PublicUrl"] = $"https://example.com/docs/{context.Data["DocumentId"]}";
                                await Task.CompletedTask;
                            });
                    })
                    .AddAction(action => {
                        action.WithName("Notify Author")
                            .WithHandler(async context => {
                                var notifier = context.ServiceProvider.GetRequiredService<NotificationActionHandler>();
                                await notifier.NotifyAsync("author@example.com", "Your document has been published");
                                System.Console.WriteLine("Action: Author notified");
                            });
                    });
            });
    });
}
        
static void DisplayWorkflow(IWorkflow workflow)
{
    System.Console.WriteLine("\nWorkflow Steps:");
    System.Console.WriteLine("---------------");
            
    var steps = workflow.Steps as IEnumerable<IWorkflowStep>;
    int stepNumber = 1;
            
    foreach (var step in steps)
    {
        System.Console.WriteLine($"{stepNumber}. {step.Name}");
        System.Console.WriteLine($"   Required Roles: {string.Join(", ", step.RequiredRoles)}");
                
        System.Console.WriteLine("   Actions:");
        foreach (var action in step.Actions)
        {
            System.Console.WriteLine($"   - {action.Name}");
        }
                
        stepNumber++;
        System.Console.WriteLine();
    }
            
    System.Console.WriteLine($"Current Step: {workflow.CurrentStep.Name}");
}
        
static async Task ProcessWorkflowAsync(IWorkflow workflow, IWorkflowService workflowService)
{
    System.Console.WriteLine("\nProcessing Workflow:");
    System.Console.WriteLine("-------------------");
            
    // Create sample data
    var data = new Dictionary<string, object>();
            
    // Step 1: Author creates draft
    System.Console.WriteLine("\nStep 1: Author creates draft");
    await workflowService.MoveWorkflowToNextStepAsync(workflow.Id, "user1", "Author", data);
    System.Console.WriteLine($"Current step: {(await workflowService.GetWorkflowAsync(workflow.Id)).CurrentStep.Name}");
    System.Console.WriteLine("Data added:");
    foreach (var item in data)
    {
        System.Console.WriteLine($"- {item.Key}: {item.Value}");
    }
            
    // Step 2: Reviewer reviews
    System.Console.WriteLine("\nStep 2: Reviewer reviews document");
    await workflowService.MoveWorkflowToNextStepAsync(workflow.Id, "user2", "Reviewer", data);
    System.Console.WriteLine($"Current step: {(await workflowService.GetWorkflowAsync(workflow.Id)).CurrentStep.Name}");
    System.Console.WriteLine("Data added:");
    foreach (var item in data)
    {
        if (item.Key.StartsWith("Review"))
        {
            System.Console.WriteLine($"- {item.Key}: {item.Value}");
        }
    }
            
    // Step 3: Manager approves
    System.Console.WriteLine("\nStep 3: Manager approves document");
    await workflowService.MoveWorkflowToNextStepAsync(workflow.Id, "user3", "Manager", data);
    System.Console.WriteLine($"Current step: {(await workflowService.GetWorkflowAsync(workflow.Id)).CurrentStep.Name}");
    System.Console.WriteLine("Data added:");
    foreach (var item in data)
    {
        if (item.Key.StartsWith("Approv"))
        {
            System.Console.WriteLine($"- {item.Key}: {item.Value}");
        }
    }
            
    // Step 4: Publisher publishes
    System.Console.WriteLine("\nStep 4: Publisher publishes document");
    await workflowService.MoveWorkflowToNextStepAsync(workflow.Id, "user4", "Publisher", data);
            
    // Final data
    System.Console.WriteLine("\nFinal workflow data:");
    foreach (var item in data)
    {
        System.Console.WriteLine($"- {item.Key}: {item.Value}");
    }
            
    System.Console.WriteLine("\nWorkflow completed!");
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
    
public class NotificationActionHandler
{
    public Task NotifyAsync(string recipient, string message)
    {
        System.Console.WriteLine($"NOTIFICATION to {recipient}: {message}");
        return Task.CompletedTask;
    }
}