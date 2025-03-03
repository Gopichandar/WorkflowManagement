namespace WorkflowManagement.Core.Models;

public class WorkflowActionDefinition
{
    public string ActionType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}