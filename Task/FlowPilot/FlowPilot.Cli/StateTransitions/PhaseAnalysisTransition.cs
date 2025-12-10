using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.StateTransitions;

/// <summary>
/// Transition to the phase-analysis phase.
/// </summary>
public class PhaseAnalysisTransition : IStateTransition
{
  private readonly PlanManager _planManager;

  public PhaseAnalysisTransition(PlanManager planManager)
  {
    _planManager = planManager;
  }

  public bool CanTransition(PlanContext context)
  {
    return context.State.HasKeyDecisions && !context.State.HasPhaseAnalysis;
  }

  public void Execute(PlanContext context)
  {
    _planManager.UpdateStateChecklist(context.PlanName, "phase-analysis", true);
    _planManager.CopyTemplateToMeta(context.PlanName, "phase-analysis.md");

    Console.WriteLine("✓ Advanced to [phase-analysis] phase");
    Console.WriteLine();
    Console.WriteLine("Instructions:");
    Console.WriteLine($"Update .flowpilot/plans/{context.PlanName}/meta/phase-analysis.md");
    Console.WriteLine("Based on the contents of goal.md, system-analysis.md, and key-decisions.md,");
    Console.WriteLine("define the high-level phases for this plan.");
    Console.WriteLine();
    Console.WriteLine("⚠️  Note: A new branch is required to proceed past phase-analysis.");
    Console.WriteLine("After committing, merge this branch before continuing with phase-details.");
  }
}
