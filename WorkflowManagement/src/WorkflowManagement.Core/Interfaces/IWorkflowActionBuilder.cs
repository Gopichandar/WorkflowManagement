namespace WorkflowManagement.Core;

public interface IWorkflowActionBuilder
{
    IWorkflowActionBuilder WithName(string name);
    IWorkflowActionBuilder WithHandler(Func<WorkflowContext, Task> handler);
    IWorkflowAction Build();
}