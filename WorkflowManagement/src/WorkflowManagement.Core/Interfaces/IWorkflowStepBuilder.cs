namespace WorkflowManagement.Core;

public interface IWorkflowStepBuilder
{
    IWorkflowStepBuilder WithName(string name);
    IWorkflowStepBuilder RequireRole(string roleId);
    IWorkflowStepBuilder AddAction(Action<IWorkflowActionBuilder> actionBuilderAction);
    IWorkflowStep Build();
}
