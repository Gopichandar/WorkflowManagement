namespace WorkflowManagement.Core;


public class WorkflowAction : IWorkflowAction
{
    private readonly Func<WorkflowContext, Task> _handler;

    public string Id { get; private set; }
    public string Name { get; private set; }

    public WorkflowAction(string name, Func<WorkflowContext, Task> handler)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        _handler = handler;
    }

    public async Task ExecuteAsync(WorkflowContext context)
    {
        await _handler(context);
    }
}
