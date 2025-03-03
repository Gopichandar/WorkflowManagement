using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowManagement.Core.Models;

namespace WorkflowManagement.Builder;
public class WorkflowBlueprintBuilder : IWorkflowBlueprintBuilder
{
    private readonly WorkflowBlueprint _blueprint;

    public WorkflowBlueprintBuilder()
    {
        _blueprint = new WorkflowBlueprint
        {
            Version = 1
        };
    }

    public IWorkflowBlueprintBuilder WithName(string name)
    {
        _blueprint.Name = name;
        return this;
    }

    public IWorkflowBlueprintBuilder WithVersion(int version)
    {
        _blueprint.Version = version;
        return this;
    }

    public IWorkflowBlueprintBuilder AddStep(Action<IWorkflowStepDefinitionBuilder> stepBuilderAction)
    {
        var stepBuilder = new WorkflowStepDefinitionBuilder();
        stepBuilderAction(stepBuilder);
        var step = stepBuilder.Build();
        _blueprint.StepDefinitions.Add(step);
        return this;
    }

    public WorkflowBlueprint Build()
    {
        return _blueprint;
    }
}
