namespace WorkflowManagement.Core;

public class WorkflowContext
{
    public string UserId { get; set; }
    public IEnumerable<string> UserRoles { get; set; }
    public IDictionary<string, object> Data { get; set; }
    public IServiceProvider ServiceProvider { get; set; }
}
