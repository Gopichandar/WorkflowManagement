using WorkflowManagement.Core;

namespace WorkflowManagement.Persistence;


public class InMemoryWorkflowRepository : IWorkflowRepository
{
    private readonly Dictionary<string, IWorkflow> _workflows = new();

    public Task DeleteWorkflowAsync(string workflowId)
    {
        if (_workflows.ContainsKey(workflowId))
            _workflows.Remove(workflowId);

        return Task.CompletedTask;
    }

    public Task<IWorkflow> GetWorkflowAsync(string workflowId)
    {
        _workflows.TryGetValue(workflowId, out var workflow);
        return Task.FromResult(workflow);
    }

    public Task<IEnumerable<IWorkflow>> GetWorkflowsAsync()
    {
        return Task.FromResult(_workflows.Values.AsEnumerable());
    }

    public Task SaveWorkflowAsync(IWorkflow workflow)
    {
        _workflows[workflow.Id] = workflow;
        return Task.CompletedTask;
    }
}