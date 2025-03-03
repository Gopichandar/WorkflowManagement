using WorkflowManagement.Builder;
using WorkflowManagement.Core;
using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IWorkflowRepository _repository;
    private readonly IWorkflowBlueprintRepository _blueprintRepository; 
    private readonly IServiceProvider _serviceProvider;

    public WorkflowService(IWorkflowRepository repository, IWorkflowBlueprintRepository blueprintRepository, IServiceProvider serviceProvider)
    {
        _repository = repository;
        _blueprintRepository = blueprintRepository;
        _serviceProvider = serviceProvider;
    }

    // Overload 1: Create a workflow instance using an existing blueprint.
    public async Task<IWorkflow> CreateWorkflowInstanceAsync(string blueprintId, Action<IWorkflowBuilder> builderAction)
    {
        // Retrieve the blueprint from the blueprint repository.
        var blueprint = await _blueprintRepository.GetBlueprintByIdAsync(blueprintId);
        if (blueprint == null)
            throw new KeyNotFoundException("Blueprint not found");

        var builder = new WorkflowBuilder();
        builder.UseBlueprint(blueprint);  // Allow the builder to incorporate blueprint details.
        builderAction(builder);
        var workflow = builder.Build();        
        
        await _repository.SaveWorkflowAsync(workflow);
        return workflow;
    }

    // Overload 2: Create and persist a new blueprint along with the workflow instance.
    public async Task<(WorkflowBlueprint Blueprint, IWorkflow Workflow)> CreateWorkflowWithNewBlueprintAsync(
        WorkflowBlueprint blueprint, Action<IWorkflowBuilder> builderAction)
    {
        // Save the new blueprint.
        await _blueprintRepository.SaveBlueprintAsync(blueprint);

        var builder = new WorkflowBuilder();
        builder.UseBlueprint(blueprint);  // Optionally pre-populate the builder.
        builderAction(builder);
        var workflow = builder.Build();

        // Associate the new workflow with the new blueprint.        
        await _repository.SaveWorkflowAsync(workflow);
        return (blueprint, workflow);
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

        await workflow.MoveToNextStepAsync(userId, roleId, data, _serviceProvider);
        await _repository.SaveWorkflowAsync(workflow);
    }
}
