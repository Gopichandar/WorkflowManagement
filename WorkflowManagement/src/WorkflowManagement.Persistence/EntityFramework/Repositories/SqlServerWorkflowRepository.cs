using Microsoft.EntityFrameworkCore;
using WorkflowManagement.Core;
using WorkflowManagement.Persistence.EntityFramework.Entities;

namespace WorkflowManagement.Persistence;


// Repository implementations
public class SqlServerWorkflowRepository : IWorkflowRepository
{
    private readonly WorkflowDbContext _dbContext;
    private readonly IServiceProvider _serviceProvider;

    public SqlServerWorkflowRepository(WorkflowDbContext dbContext, IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _serviceProvider = serviceProvider;
    }

    public async Task DeleteWorkflowAsync(string workflowId)
    {
        var entity = await _dbContext.Workflows.FindAsync(workflowId);
        if (entity != null)
        {
            _dbContext.Workflows.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<IWorkflow> GetWorkflowAsync(string workflowId)
    {
        var entity = await _dbContext.Workflows
            .Include(w => w.Steps)
                .ThenInclude(s => s.Roles)
            .Include(w => w.Steps)
                .ThenInclude(s => s.Actions)
            .FirstOrDefaultAsync(w => w.Id == workflowId);

        if (entity == null)
            return null;

        return MapEntityToWorkflow(entity);
    }

    public async Task<IEnumerable<IWorkflow>> GetWorkflowsAsync()
    {
        var entities = await _dbContext.Workflows
            .Include(w => w.Steps)
                .ThenInclude(s => s.Roles)
            .Include(w => w.Steps)
                .ThenInclude(s => s.Actions)
            .ToListAsync();

        return entities.Select(MapEntityToWorkflow);
    }

    public async Task SaveWorkflowAsync(IWorkflow workflow)
    {
        var entity = MapWorkflowToEntity(workflow);

        var existing = await _dbContext.Workflows.FindAsync(entity.Id);
        if (existing == null)
        {
            _dbContext.Workflows.Add(entity);
        }
        else
        {
            _dbContext.Entry(existing).CurrentValues.SetValues(entity);
        }

        await _dbContext.SaveChangesAsync();
    }

    private Workflow MapEntityToWorkflow(WorkflowEntity entity)
    {
        var workflow = new Workflow(entity.Name);

        // Map properties using reflection or a mapper library
        // This is simplified for brevity

        return workflow;
    }

    private WorkflowEntity MapWorkflowToEntity(IWorkflow workflow)
    {
        var entity = new WorkflowEntity
        {
            Id = workflow.Id,
            Name = workflow.Name,
            CurrentStepId = workflow.CurrentStep?.Id
        };

        // Map steps, roles, and actions using reflection or a mapper library
        // This is simplified for brevity

        return entity;
    }
}
