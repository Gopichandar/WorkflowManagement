namespace WorkflowManagement.Core;

public interface IWorkflowBuilder
{
    IWorkflowBuilder WithName(string name);
    IWorkflowBuilder AddStep(Action<IWorkflowStepBuilder> stepBuilderAction);
    IWorkflow Build();
}
