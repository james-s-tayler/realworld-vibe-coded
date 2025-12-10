using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.StateTransitions;

/// <summary>
/// Transition to the system-analysis phase.
/// </summary>
public class SystemAnalysisTransition : IStateTransition
{
  private readonly PlanManager _planManager;

  public SystemAnalysisTransition(PlanManager planManager)
  {
    _planManager = planManager;
  }

  public bool CanTransition(PlanContext context)
  {
    return context.State.HasReferences && !context.State.HasSystemAnalysis;
  }

  public void Execute(PlanContext context)
  {
    _planManager.UpdateStateChecklist(context.PlanName, "system-analysis", true);
    _planManager.CopyTemplateToMeta(context.PlanName, "system-analysis.md");

    Console.WriteLine("✓ Advanced to [system-analysis] phase");
    Console.WriteLine();
    Console.WriteLine("Instructions:");
    Console.WriteLine($"Update .flowpilot/plans/{context.PlanName}/meta/system-analysis.md");
    Console.WriteLine("Analyze the current parts of the system that are relevant to the goal");
    Console.WriteLine("stated in goal.md and record them in system-analysis.md.");
  }
}
