using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Builder;

public interface IWorkflowBlueprintBuilder
{
    IWorkflowBlueprintBuilder WithName(string name);
    IWorkflowBlueprintBuilder WithVersion(int version);
    IWorkflowBlueprintBuilder AddStep(Action<IWorkflowStepDefinitionBuilder> stepBuilderAction);
    WorkflowBlueprint Build();
}
