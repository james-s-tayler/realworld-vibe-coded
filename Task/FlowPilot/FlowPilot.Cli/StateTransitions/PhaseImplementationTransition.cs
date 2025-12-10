using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.StateTransitions;

/// <summary>
/// Transition to the next phase implementation.
/// </summary>
public class PhaseImplementationTransition : IStateTransition
{
  private readonly PlanManager _planManager;

  public PhaseImplementationTransition(PlanManager planManager)
  {
    _planManager = planManager;
  }

  public bool CanTransition(PlanContext context)
  {
    return context.State.HasPhaseDetails;
  }

  public void Execute(PlanContext context)
  {
    // Find the next uncompleted phase
    var nextPhase = context.State.Phases.FirstOrDefault(p => !p.IsComplete);

    if (nextPhase == null)
    {
      Console.WriteLine("✓ All phases complete! Plan finished.");
      return;
    }

    _planManager.UpdateStateChecklist(context.PlanName, $"phase_{nextPhase.PhaseNumber}", true);

    Console.WriteLine($"✓ Advanced to phase {nextPhase.PhaseNumber}: {nextPhase.PhaseName}");
    Console.WriteLine();
    Console.WriteLine("Instructions:");
    Console.WriteLine($"Implement phase {nextPhase.PhaseNumber} as described in:");
    Console.WriteLine($".flowpilot/plans/{context.PlanName}/plan/phase-{nextPhase.PhaseNumber}-details.md");
    Console.WriteLine();
    Console.WriteLine("When phase verification criteria is met, run 'flowpilot next' again.");
  }
}
