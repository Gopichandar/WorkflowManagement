using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Core;

public class Workflow : IWorkflow
{
    public string Id { get; private set; }
    public string Name { get; set; }
    public bool IsCompleted { get; private set; }
    public string CurrentStepId { get; set; }
    public WorkflowStepDefinition CurrentStep => Blueprint?.StepDefinitions.FirstOrDefault(s => s.Id == CurrentStepId);
    public WorkflowBlueprint Blueprint { get; set; }

    // Store workflow state/data
    public Dictionary<string, object> WorkflowData { get; set; } = new();

    public Workflow(string name)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
    }

    public async Task<bool> CanMoveToNextStepAsync(string userId, string roleId)
    {
        if (IsCompleted || CurrentStep == null)
            return false;

        //var nextStep = Blueprint.GetNextStep(CurrentStepId);
        //if (nextStep == null)
        //    return false;

        return CurrentStep.RequiredRoles.Contains(roleId);
    }

    public async Task MoveToNextStepAsync(string userId, string roleId, IDictionary<string, object> data, IServiceProvider serviceProvider)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Workflow is already completed");

        if (!await CanMoveToNextStepAsync(userId, roleId))
            throw new UnauthorizedAccessException("User does not have permission to move to the next step");

        var context = new WorkflowContext
        {
            UserId = userId,
            UserRoles = new[] { roleId },
            Data = data,
            ServiceProvider = serviceProvider
        };

        // Execute exit actions for current step
        if (CurrentStep != null)
        {
            await ExecuteActionsAsync(CurrentStep.ExitActions, context);
        }

        // Move to next step
        var nextStep = Blueprint.GetNextStep(CurrentStepId);
        if (nextStep == null)
        {
            IsCompleted = true;
            return;
        }

        CurrentStepId = nextStep.Id;

        // Execute entry actions for new step
        await ExecuteActionsAsync(nextStep.EntryActions, context);
    }

    private async Task ExecuteActionsAsync(List<WorkflowActionDefinition> actions, WorkflowContext context)
    {
        if (actions == null || !actions.Any())
            return;

        // Here you would implement the actual execution of actions
        // This could involve looking up action handlers from a registry
        // and invoking them with the provided context

        foreach (var action in actions)
        {
            // Example implementation:
            // var actionHandler = context.ServiceProvider.GetService<IActionHandlerFactory>().GetHandler(action.ActionType);
            // await actionHandler.ExecuteAsync(action.Parameters, context);
        }
    }
}