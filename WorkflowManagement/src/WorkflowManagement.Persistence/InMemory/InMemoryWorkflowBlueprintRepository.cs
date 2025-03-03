using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowManagement.Core.Models;
using WorkflowManagement.Core;

namespace WorkflowManagement.Persistence.InMemory;
public class InMemoryWorkflowBlueprintRepository : IWorkflowBlueprintRepository
{
    private readonly ConcurrentDictionary<string, WorkflowBlueprint> _blueprints = new();

    public Task SaveBlueprintAsync(WorkflowBlueprint blueprint)
    {
        if (blueprint == null)
            throw new ArgumentNullException(nameof(blueprint));

        _blueprints.AddOrUpdate(blueprint.Id, blueprint, (id, existing) => blueprint);
        return Task.CompletedTask;
    }

    public Task<WorkflowBlueprint> GetBlueprintByIdAsync(string blueprintId)
    {
        _blueprints.TryGetValue(blueprintId, out var blueprint);
        return Task.FromResult(blueprint);
    }

    public Task<WorkflowBlueprint> GetBlueprintByNameAsync(string name)
    {
        var blueprint = _blueprints.Values.FirstOrDefault(bp =>
            bp.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(blueprint);
    }

    public Task<IEnumerable<WorkflowBlueprint>> GetBlueprintsAsync()
    {
        return Task.FromResult<IEnumerable<WorkflowBlueprint>>(_blueprints.Values);
    }
}
