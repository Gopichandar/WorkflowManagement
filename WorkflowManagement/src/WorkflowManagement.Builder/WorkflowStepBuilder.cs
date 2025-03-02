using WorkflowManagement.Core;

namespace WorkflowManagement.Builder;


// Implement the WorkflowStepBuilder
public class WorkflowStepBuilder : IWorkflowStepBuilder
{
    private readonly WorkflowStep _step;

    public WorkflowStepBuilder()
    {
        _step = new WorkflowStep("New Step");
    }

    public IWorkflowStepBuilder WithName(string name)
    {
        _step.Name = name;
        return this;
    }

    public IWorkflowStepBuilder RequireRole(string roleId)
    {
        _step.AddRole(roleId);
        return this;
    }

    public IWorkflowStepBuilder AddAction(Action<IWorkflowActionBuilder> actionBuilderAction)
    {
        var actionBuilder = new WorkflowActionBuilder();
        actionBuilderAction(actionBuilder);

        var action = actionBuilder.Build();
        _step.AddAction((WorkflowAction)action);

        return this;
    }

    public IWorkflowStep Build()
    {
        return _step;
    }
}