using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Builder;

public class ChainStepBuilder : IChainStepBuilder
{
    private readonly WorkflowBlueprintBuilder _workflowBuilder;
    private readonly WorkflowStepDefinition _step;

    internal ChainStepBuilder(WorkflowBlueprintBuilder workflowBuilder, WorkflowStepDefinition step)
    {
        _workflowBuilder = workflowBuilder;
        _step = step;
    }

    public IChainStepBuilder AsInitial()
    {
        _step.IsInitial = true;
        return this;
    }

    public IChainStepBuilder RequireRole(string roleId)
    {
        _step.RequiredRoles.Add(roleId);
        return this;
    }

    public IChainStepBuilder AddEntryAction(string actionType, Dictionary<string, object> parameters)
    {
        if (_step.EntryActions == null)
        {
            _step.EntryActions = new List<WorkflowActionDefinition>();
        }

        _step.EntryActions.Add(new WorkflowActionDefinition
        {
            ActionType = actionType,
            Parameters = parameters
        });

        return this;
    }

    public IChainStepBuilder AddExitAction(string actionType, Dictionary<string, object> parameters)
    {
        if (_step.ExitActions == null)
        {
            _step.ExitActions = new List<WorkflowActionDefinition>();
        }

        _step.ExitActions.Add(new WorkflowActionDefinition
        {
            ActionType = actionType,
            Parameters = parameters
        });

        return this;
    }

    public IChainStepBuilder ThenStep(string name)
    {
        // Return to the workflow builder to create the next step
        return _workflowBuilder.ThenStep(name);
    }

    public WorkflowBlueprint Build()
    {
        // Return to the workflow builder to finalize the build
        return _workflowBuilder.Build();
    }
}