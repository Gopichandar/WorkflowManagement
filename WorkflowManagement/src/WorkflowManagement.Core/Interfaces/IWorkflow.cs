using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Core;

public interface IWorkflow
{
    string Id { get; }
    string Name { get; }
    bool IsCompleted { get; }
    string CurrentStepId { get; }
    WorkflowStepDefinition CurrentStep { get; }
    WorkflowBlueprint Blueprint { get; }
    Task<bool> CanMoveToNextStepAsync(string userId, string roleId);
    Task MoveToNextStepAsync(string userId, string roleId, IDictionary<string, object> data, IServiceProvider serviceProvider);
}