namespace WorkflowManagement.Extensions;

public class WorkflowOptions
{
    public bool UseInMemoryStorage { get; set; }
    public bool UseSqlServer { get; set; }
    public string SqlConnectionString { get; set; }
}
