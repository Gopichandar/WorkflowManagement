using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Core;

public interface IWorkflowBlueprintRepository
{
    Task SaveBlueprintAsync(WorkflowBlueprint blueprint);
    Task<WorkflowBlueprint> GetBlueprintByIdAsync(string blueprintId);
    Task<WorkflowBlueprint> GetBlueprintByNameAsync(string name);
    Task<IEnumerable<WorkflowBlueprint>> GetBlueprintsAsync();
}
