using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Core;

public interface IWorkflowBuilder
{
    IWorkflowBuilder WithName(string name);
    IWorkflowBuilder UseBlueprint(WorkflowBlueprint blueprint);
    IWorkflow Build();
}
