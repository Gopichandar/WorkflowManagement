namespace WorkflowManagement.Core.Models;

public class WorkflowStepDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public bool IsInitial { get; set; }
    public string NextStepId { get; set; }
    public List<string> RequiredRoles { get; set; } = new();
    public List<WorkflowActionDefinition> EntryActions { get; set; } = new();
    public List<WorkflowActionDefinition> ExitActions { get; set; } = new();
}
