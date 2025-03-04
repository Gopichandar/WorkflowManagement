using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Builder;

public interface IChainStepBuilder
{
    IChainStepBuilder AddEntryAction(string actionType, Dictionary<string, object> parameters);
    IChainStepBuilder AddExitAction(string actionType, Dictionary<string, object> parameters);
    IChainStepBuilder AsInitial();
    WorkflowBlueprint Build();
    IChainStepBuilder RequireRole(string roleId);
    IChainStepBuilder ThenStep(string name);
}