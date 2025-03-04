using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowManagement.Core.Models;
public class WorkflowBlueprint
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public int Version { get; set; }
    public List<WorkflowStepDefinition> StepDefinitions { get; set; } = new();

    public RejectionBehavior RejectionBehavior { get; set; } = RejectionBehavior.ReturnToPreviousStep;

    public WorkflowStepDefinition GetInitialStep()
    {
        return StepDefinitions.FirstOrDefault(s => s.IsInitial);
    }

    public WorkflowStepDefinition GetNextStep(string currentStepId)
    {
        var currentStep = StepDefinitions.FirstOrDefault(s => s.Id == currentStepId);
        if (currentStep == null || string.IsNullOrEmpty(currentStep.NextStepId))
            return null;

        return StepDefinitions.FirstOrDefault(s => s.Id == currentStep.NextStepId);
    }
}

public enum RejectionBehavior
{
    ReturnToPreviousStep,
    ResetToDraft
}
