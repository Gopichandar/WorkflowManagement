using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Builder;

public interface IWorkflowBlueprintBuilder
{
    IChainStepBuilder BeginStep(string name);
    WorkflowBlueprint Build();

    //IWorkflowBlueprintBuilder WithName(string name);
    //IWorkflowBlueprintBuilder WithVersion(int version);
    //IWorkflowBlueprintBuilder AddStep(Action<IWorkflowStepDefinitionBuilder> stepBuilderAction);
    //WorkflowBlueprint Build();
    IWorkflowBlueprintBuilder WithName(string name);
    IWorkflowBlueprintBuilder WithVersion(int version);
}
