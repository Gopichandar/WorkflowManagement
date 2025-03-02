using Microsoft.EntityFrameworkCore;
using WorkflowManagement.Persistence.EntityFramework.Entities;

namespace WorkflowManagement.Persistence;


// Database implementations
public class WorkflowDbContext : DbContext
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options) { }

    public DbSet<WorkflowEntity> Workflows { get; set; }
    public DbSet<WorkflowStepEntity> Steps { get; set; }
    public DbSet<WorkflowActionEntity> Actions { get; set; }
    public DbSet<WorkflowStepRoleEntity> StepRoles { get; set; }
}
