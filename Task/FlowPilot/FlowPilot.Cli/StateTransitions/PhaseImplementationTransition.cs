using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.StateTransitions;

/// <summary>
/// Transition to the next phase implementation.
/// </summary>
public class PhaseImplementationTransition : IStateTransition
{
  private readonly PlanManager _planManager;
  private readonly ILogger<PhaseImplementationTransition> _logger;

  public PhaseImplementationTransition(PlanManager planManager, ILogger<PhaseImplementationTransition> logger)
  {
    _planManager = planManager;
    _logger = logger;
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
      _logger.LogInformation("✓ All phases complete! Plan finished.");
      return;
    }

    _planManager.UpdateStateChecklist(context.PlanName, $"phase_{nextPhase.PhaseNumber}", true);

    _logger.LogInformation("✓ Advanced to phase {PhaseNumber}: {PhaseName}", nextPhase.PhaseNumber, nextPhase.PhaseName);
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Instructions:");
    _logger.LogInformation("Implement phase {PhaseNumber} as described in:", nextPhase.PhaseNumber);
    _logger.LogInformation(".flowpilot/plans/{PlanName}/plan/phase-{PhaseNumber}-details.md", context.PlanName, nextPhase.PhaseNumber);
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("When phase verification criteria is met, run 'flowpilot next' again.");
  }
}
