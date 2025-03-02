namespace WorkflowManagement.Core;

public interface IWorkflow
{
    string Id { get; }
    string Name { get; }
    IEnumerable<IWorkflowStep> Steps { get; }
    IWorkflowStep CurrentStep { get; }
    Task MoveToNextStepAsync(string userId, string roleId, IDictionary<string, object> data, IServiceProvider serviceProvider);
    Task<bool> CanMoveToNextStepAsync(string userId, string roleId);
}
