namespace WorkflowManagement.Core;


public class Workflow : IWorkflow
{
    private List<WorkflowStep> _steps = new();
    private string _currentStepId;

    public string Id { get; private set; }
    public string Name { get; set; }
    public IEnumerable<IWorkflowStep> Steps => _steps;

    public IWorkflowStep CurrentStep => _steps.FirstOrDefault(s => s.Id == _currentStepId);

    public Workflow(string name)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
    }

    public void AddStep(WorkflowStep step)
    {
        _steps.Add(step);

        // Set as current step if this is the first step
        if (_steps.Count == 1)
        {
            _currentStepId = step.Id;
        }
    }

    public async Task<bool> CanMoveToNextStepAsync(string userId, string roleId)
    {
        var currentStepIndex = _steps.FindIndex(s => s.Id == _currentStepId);
        if (currentStepIndex >= _steps.Count - 1)
            return false;

        var currentStep = _steps[currentStepIndex];
        return currentStep.RequiredRoles.Contains(roleId);
    }

    public async Task MoveToNextStepAsync(string userId, string roleId, IDictionary<string, object> data)
    {
        if (!await CanMoveToNextStepAsync(userId, roleId))
            throw new UnauthorizedAccessException("User does not have permission to move to the next step");

        var context = new WorkflowContext
        {
            UserId = userId,
            UserRoles = new[] { roleId },
            Data = data
        };

        var currentStepIndex = _steps.FindIndex(s => s.Id == _currentStepId);

        // Execute exit actions for current step
        await _steps[currentStepIndex].ExecuteAsync(context);

        // Move to next step
        _currentStepId = _steps[currentStepIndex + 1].Id;

        // Execute entry actions for new step
        await _steps[currentStepIndex + 1].ExecuteAsync(context);
    }
}