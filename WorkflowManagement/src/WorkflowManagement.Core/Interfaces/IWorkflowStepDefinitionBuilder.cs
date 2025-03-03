namespace WorkflowManagement.Builder;

public interface IWorkflowStepDefinitionBuilder
{
    IWorkflowStepDefinitionBuilder WithName(string name);
    IWorkflowStepDefinitionBuilder AsInitial();
    IWorkflowStepDefinitionBuilder WithNextStep(string nextStepId);
    IWorkflowStepDefinitionBuilder RequireRole(string roleId);
    IWorkflowStepDefinitionBuilder AddEntryAction(string actionType, Dictionary<string, object> parameters);
    IWorkflowStepDefinitionBuilder AddExitAction(string actionType, Dictionary<string, object> parameters);
}
