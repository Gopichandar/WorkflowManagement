using WorkflowManagement.Core;

namespace WorkflowManagement.Services;

public interface IWorkflowService
{
    Task<IWorkflow> CreateWorkflowAsync(Action<IWorkflowBuilder> builderAction);
    Task<IWorkflow> GetWorkflowAsync(string workflowId);
    Task<IEnumerable<IWorkflow>> GetWorkflowsAsync();
    Task MoveWorkflowToNextStepAsync(string workflowId, string userId, string roleId, IDictionary<string, object> data);
}
