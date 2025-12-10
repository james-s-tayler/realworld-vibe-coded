using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.StateTransitions;

/// <summary>
/// Transition to the key-decisions phase.
/// </summary>
public class KeyDecisionsTransition : IStateTransition
{
  private readonly PlanManager _planManager;

  public KeyDecisionsTransition(PlanManager planManager)
  {
    _planManager = planManager;
  }

  public bool CanTransition(PlanContext context)
  {
    return context.State.HasSystemAnalysis && !context.State.HasKeyDecisions;
  }

  public void Execute(PlanContext context)
  {
    _planManager.UpdateStateChecklist(context.PlanName, "key-decisions", true);
    _planManager.CopyTemplateToMeta(context.PlanName, "key-decisions.md");

    Console.WriteLine("✓ Advanced to [key-decisions] phase");
    Console.WriteLine();
    Console.WriteLine("Instructions:");
    Console.WriteLine($"Update .flowpilot/plans/{context.PlanName}/meta/key-decisions.md");
    Console.WriteLine("Document any decisions that need to be made based on the contents of");
    Console.WriteLine("goal.md, references.md, and system-analysis.md.");
    Console.WriteLine();
    Console.WriteLine("⚠️  Note: A new branch is required to proceed past key-decisions.");
    Console.WriteLine("After committing, merge this branch before continuing with phase-analysis.");
  }
}
