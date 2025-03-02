using WorkflowManagement.Core;

namespace WorkflowManagement.Builder;


// Implement the WorkflowActionBuilder
public class WorkflowActionBuilder : IWorkflowActionBuilder
{
    private string _name = "New Action";
    private Func<WorkflowContext, Task> _handler;

    public IWorkflowActionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public IWorkflowActionBuilder WithHandler(Func<WorkflowContext, Task> handler)
    {
        _handler = handler;
        return this;
    }

    public IWorkflowAction Build()
    {
        if (_handler == null)
        {
            _handler = _ => Task.CompletedTask;
        }

        return new WorkflowAction(_name, _handler);
    }
}