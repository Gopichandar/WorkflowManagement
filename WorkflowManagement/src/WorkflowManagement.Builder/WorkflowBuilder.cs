using WorkflowManagement.Core;

namespace WorkflowManagement.Builder;


public class WorkflowBuilder : IWorkflowBuilder
{
    private readonly Workflow _workflow;

    public WorkflowBuilder()
    {
        _workflow = new Workflow("New Workflow");
    }

    public IWorkflowBuilder WithName(string name)
    {
        _workflow.Name = name;
        return this;
    }

    public IWorkflowBuilder AddStep(Action<IWorkflowStepBuilder> stepBuilderAction)
    {
        var stepBuilder = new WorkflowStepBuilder();
        stepBuilderAction(stepBuilder);

        var step = stepBuilder.Build();
        _workflow.AddStep((WorkflowStep)step);

        return this;
    }

    public IWorkflow Build()
    {
        return _workflow;
    }
}
