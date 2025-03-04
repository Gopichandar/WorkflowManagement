using WorkflowManagement.Core;
using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Services;

public interface IWorkflowService
{
    Task<IWorkflow> CreateWorkflowAsync(Action<IWorkflowBuilder> builderAction);
    Task<IWorkflow> CreateWorkflowInstanceAsync(string blueprintId, Action<IWorkflowBuilder> builderAction);
    Task<(WorkflowBlueprint Blueprint, IWorkflow Workflow)> CreateWorkflowWithNewBlueprintAsync(WorkflowBlueprint blueprint, Action<IWorkflowBuilder> builderAction);
    Task<IWorkflow> GetWorkflowAsync(string workflowId);
    Task<IEnumerable<IWorkflow>> GetWorkflowsAsync();
    Task MoveWorkflowToNextStepAsync(string workflowId, string userId, string roleId, IDictionary<string, object> data);
    Task RejectWorkflowStepAsync(string workflowId, string userId, string roleId, string rejectionReason, IDictionary<string, object> data);
}
