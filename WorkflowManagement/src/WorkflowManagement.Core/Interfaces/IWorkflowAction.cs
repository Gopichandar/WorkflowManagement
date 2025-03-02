namespace WorkflowManagement.Core;

public interface IWorkflowAction
{
    string Id { get; }
    string Name { get; }
    Task ExecuteAsync(WorkflowContext context);
}

