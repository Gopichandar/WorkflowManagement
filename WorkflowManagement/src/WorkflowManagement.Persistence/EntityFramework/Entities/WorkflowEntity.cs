using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowManagement.Persistence.EntityFramework.Entities;

// Entity classes for database storage
public class WorkflowEntity
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string CurrentStepId { get; set; }
    public List<WorkflowStepEntity> Steps { get; set; } = new();
}

public class WorkflowStepEntity
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string WorkflowId { get; set; }
    public List<WorkflowStepRoleEntity> Roles { get; set; } = new();
    public List<WorkflowActionEntity> Actions { get; set; } = new();
}

public class WorkflowStepRoleEntity
{
    public int Id { get; set; }
    public string RoleId { get; set; }
    public string StepId { get; set; }
}

public class WorkflowActionEntity
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string HandlerTypeName { get; set; }
    public string StepId { get; set; }
}
