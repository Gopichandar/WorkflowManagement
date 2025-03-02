namespace WorkflowManagement.Core;

public interface IWorkflowStep
{
    string Id { get; }
    string Name { get; }
    IEnumerable<string> RequiredRoles { get; }
    IEnumerable<IWorkflowAction> Actions { get; }
    Task ExecuteAsync(WorkflowContext context);
    bool CanExecute(WorkflowContext context);
}
