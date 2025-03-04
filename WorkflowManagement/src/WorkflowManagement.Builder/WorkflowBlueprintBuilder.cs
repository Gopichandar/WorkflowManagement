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
    private readonly Dictionary<string, WorkflowStepDefinition> _steps = new();
    private WorkflowStepDefinition _currentStep;    

    public WorkflowBlueprintBuilder()
    {
        _blueprint = new WorkflowBlueprint
        {
            Version = 1,
            StepDefinitions = new List<WorkflowStepDefinition>()
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

    public IChainStepBuilder BeginStep(string name)
    {
        _currentStep = new WorkflowStepDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = name
        };

        _steps[name] = _currentStep;
        _blueprint.StepDefinitions.Add(_currentStep);

        return new ChainStepBuilder(this, _currentStep);
    }

    internal ChainStepBuilder ThenStep(string name)
    {
        if (_currentStep == null)
        {
            throw new InvalidOperationException("There is no current step to link from. Did you forget to call BeginStep?");
        }

        // Create the new step
        var newStep = new WorkflowStepDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = name
        };

        // Link the current step to the new one
        _currentStep.NextStepId = newStep.Id;

        // Update the current step to be the new step
        _currentStep = newStep;

        _steps[name] = _currentStep;
        _blueprint.StepDefinitions.Add(_currentStep);

        return new ChainStepBuilder(this, _currentStep);
    }

    public WorkflowBlueprint Build()
    {
        // Perform final validation
        var initialSteps = _blueprint.StepDefinitions.Count(s => s.IsInitial);
        if (initialSteps == 0)
        {
            throw new InvalidOperationException("Workflow must have at least one initial step");
        }
        else if (initialSteps > 1)
        {
            throw new InvalidOperationException("Workflow cannot have more than one initial step");
        }

        return _blueprint;
    }
}
