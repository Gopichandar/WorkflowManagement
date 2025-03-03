using WorkflowManagement.Core;
using WorkflowManagement.Core.Models;

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

    public IWorkflowBuilder UseBlueprint(WorkflowBlueprint blueprint)
    {
        _workflow.Blueprint = blueprint;

        // Set the initial step
        var initialStep = blueprint.GetInitialStep();
        if (initialStep != null)
        {
            _workflow.CurrentStepId = initialStep.Id;
        }

        return this;
    }

    public IWorkflow Build()
    {
        if (_workflow.Blueprint == null)
        {
            throw new InvalidOperationException("Workflow must have a blueprint");
        }

        return _workflow;
    }
}
