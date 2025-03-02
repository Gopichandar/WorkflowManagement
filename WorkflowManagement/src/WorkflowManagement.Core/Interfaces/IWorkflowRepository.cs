namespace WorkflowManagement.Core;

public interface IWorkflowRepository
{
    Task<IWorkflow> GetWorkflowAsync(string workflowId);
    Task<IEnumerable<IWorkflow>> GetWorkflowsAsync();
    Task SaveWorkflowAsync(IWorkflow workflow);
    Task DeleteWorkflowAsync(string workflowId);
}
