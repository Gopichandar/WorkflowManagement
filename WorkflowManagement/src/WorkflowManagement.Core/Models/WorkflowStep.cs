namespace WorkflowManagement.Core;


public class WorkflowStep : IWorkflowStep
{
    private List<string> _requiredRoles = new();
    private List<WorkflowAction> _actions = new();

    public string Id { get; private set; }
    public string Name { get; set; }
    public IEnumerable<string> RequiredRoles => _requiredRoles;
    public IEnumerable<IWorkflowAction> Actions => _actions;

    public WorkflowStep(string name)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
    }

    public void AddRole(string roleId)
    {
        _requiredRoles.Add(roleId);
    }

    public void AddAction(WorkflowAction action)
    {
        _actions.Add(action);
    }

    public bool CanExecute(WorkflowContext context)
    {
        return context.UserRoles.Any(r => _requiredRoles.Contains(r));
    }

    public async Task ExecuteAsync(WorkflowContext context)
    {
        if (!CanExecute(context))
            throw new UnauthorizedAccessException("User does not have permission to execute this step");

        foreach (var action in _actions)
        {
            await action.ExecuteAsync(context);
        }
    }
}