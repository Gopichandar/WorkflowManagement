using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Builder;

public class WorkflowStepDefinitionBuilder : IWorkflowStepDefinitionBuilder
{
    private readonly WorkflowStepDefinition _stepDefinition;

    public WorkflowStepDefinitionBuilder()
    {
        _stepDefinition = new WorkflowStepDefinition();
    }

    public IWorkflowStepDefinitionBuilder WithName(string name)
    {
        _stepDefinition.Name = name;
        return this;
    }

    public IWorkflowStepDefinitionBuilder AsInitial()
    {
        _stepDefinition.IsInitial = true;
        return this;
    }

    public IWorkflowStepDefinitionBuilder WithNextStep(string nextStepId)
    {
        _stepDefinition.NextStepId = nextStepId;
        return this;
    }

    public IWorkflowStepDefinitionBuilder RequireRole(string roleId)
    {
        _stepDefinition.RequiredRoles.Add(roleId);
        return this;
    }

    public IWorkflowStepDefinitionBuilder AddEntryAction(string actionType, Dictionary<string, object> parameters)
    {
        _stepDefinition.EntryActions.Add(new WorkflowActionDefinition
        {
            ActionType = actionType,
            Parameters = parameters
        });
        return this;
    }

    public IWorkflowStepDefinitionBuilder AddExitAction(string actionType, Dictionary<string, object> parameters)
    {
        _stepDefinition.ExitActions.Add(new WorkflowActionDefinition
        {
            ActionType = actionType,
            Parameters = parameters
        });
        return this;
    }

    public WorkflowStepDefinition Build()
    {
        return _stepDefinition;
    }
}
