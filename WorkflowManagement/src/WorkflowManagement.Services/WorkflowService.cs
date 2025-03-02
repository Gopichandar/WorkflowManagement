using WorkflowManagement.Builder;
using WorkflowManagement.Core;

namespace WorkflowManagement.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IWorkflowRepository _repository;
    private readonly IServiceProvider _serviceProvider;

    public WorkflowService(IWorkflowRepository repository, IServiceProvider serviceProvider)
    {
        _repository = repository;
        _serviceProvider = serviceProvider;
    }

    public async Task<IWorkflow> CreateWorkflowAsync(Action<IWorkflowBuilder> builderAction)
    {
        var builder = new WorkflowBuilder();
        builderAction(builder);

        var workflow = builder.Build();
        await _repository.SaveWorkflowAsync(workflow);

        return workflow;
    }

    public Task<IWorkflow> GetWorkflowAsync(string workflowId)
    {
        return _repository.GetWorkflowAsync(workflowId);
    }

    public Task<IEnumerable<IWorkflow>> GetWorkflowsAsync()
    {
        return _repository.GetWorkflowsAsync();
    }

    public async Task MoveWorkflowToNextStepAsync(string workflowId, string userId, string roleId, IDictionary<string, object> data)
    {
        var workflow = await _repository.GetWorkflowAsync(workflowId);
        if (workflow == null)
            throw new KeyNotFoundException($"Workflow with ID {workflowId} not found");

        await workflow.MoveToNextStepAsync(userId, roleId, data);
        await _repository.SaveWorkflowAsync(workflow);
    }
}
