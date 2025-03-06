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

    public async Task<bool> CanRejectStepAsync(string userId, string roleId)
    {
        if (IsCompleted || CurrentStep == null || CurrentStep.IsInitial)
            return false;

        // Check if user has rejection permission for current step
        // You might want to add a specific "CanReject" property to steps
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

    public async Task RejectStepAsync(string userId, string roleId, string rejectionReason, IDictionary<string, object> data, IServiceProvider serviceProvider)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Workflow is already completed");

        if (!await CanRejectStepAsync(userId, roleId))
            throw new UnauthorizedAccessException("User does not have permission to reject this step");

        if (CurrentStep.IsInitial)
            throw new InvalidOperationException("Cannot reject the initial step");

        var context = new WorkflowContext
        {
            UserId = userId,
            UserRoles = new[] { roleId },
            Data = data,
            ServiceProvider = serviceProvider
        };

        // Store rejection information in workflow data
        WorkflowData["LastRejection"] = new
        {
            StepId = CurrentStepId,
            Reason = rejectionReason,
            RejectedBy = userId,
            RejectedAt = DateTime.UtcNow
        };

        // Execute rejection actions for current step if defined
        if (CurrentStep != null && CurrentStep.RejectionActions != null)
        {
            await ExecuteActionsAsync(CurrentStep.RejectionActions, context);
        }

        var rejectionBehavior = Blueprint.RejectionBehavior;
        if (rejectionBehavior == RejectionBehavior.ResetToDraft)
        {           
            var initialStep = Blueprint.GetInitialStep();
            if (initialStep == null)
            {
                throw new InvalidOperationException("No initial step defined in the workflow");
            }
            CurrentStepId = initialStep.Id;

            // Execute entry actions for the initial step
            await ExecuteActionsAsync(initialStep.EntryActions, context);
        }
        else
        {
            // Go back to previous step
            // Get previous step definition
            var previousStep = Blueprint.StepDefinitions.FirstOrDefault(s => s.NextStepId == CurrentStepId);
            if (previousStep is not null)
            {
                string previousStepId = previousStep.Id;
                CurrentStepId = previousStepId;

                // Execute reentry actions if defined
                if (previousStep != null && previousStep.ReentryActions != null)
                {
                    await ExecuteActionsAsync(previousStep.ReentryActions, context);
                }
            }
        }
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