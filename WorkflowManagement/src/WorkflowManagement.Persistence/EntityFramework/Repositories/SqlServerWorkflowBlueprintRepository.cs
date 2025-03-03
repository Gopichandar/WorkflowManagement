using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowManagement.Core;
using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Persistence.EntityFramework.Repositories;
public class SqlServerWorkflowBlueprintRepository : IWorkflowBlueprintRepository
{
    public Task<WorkflowBlueprint> GetBlueprintByIdAsync(string blueprintId)
    {
        throw new NotImplementedException();
    }

    public Task<WorkflowBlueprint> GetBlueprintByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<WorkflowBlueprint>> GetBlueprintsAsync()
    {
        throw new NotImplementedException();
    }

    public Task SaveBlueprintAsync(WorkflowBlueprint blueprint)
    {
        throw new NotImplementedException();
    }
}
