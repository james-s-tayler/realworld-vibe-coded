using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.StateTransitions;

/// <summary>
/// Transition to the phase-analysis phase.
/// </summary>
public class PhaseAnalysisTransition : IStateTransition
{
  private readonly PlanManager _planManager;
  private readonly ILogger<PhaseAnalysisTransition> _logger;

  public PhaseAnalysisTransition(PlanManager planManager, ILogger<PhaseAnalysisTransition> logger)
  {
    _planManager = planManager;
    _logger = logger;
  }

  public bool CanTransition(PlanContext context)
  {
    return context.State.HasKeyDecisions && !context.State.HasPhaseAnalysis;
  }

  public void Execute(PlanContext context)
  {
    _planManager.UpdateStateChecklist(context.PlanName, "phase-analysis", true);
    _planManager.CopyTemplateToMeta(context.PlanName, "phase-analysis.md");

    _logger.LogInformation("✓ Advanced to [phase-analysis] phase");
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Instructions:");
    _logger.LogInformation("Update .flowpilot/plans/{PlanName}/meta/phase-analysis.md", context.PlanName);
    _logger.LogInformation("Based on the contents of goal.md, references.md, system-analysis.md, and key-decisions.md,");
    _logger.LogInformation("define the high-level phases for this plan.");
    _logger.LogInformation(string.Empty);
    _logger.LogWarning("A new branch is required to proceed past phase-analysis.");
    _logger.LogInformation("After committing, merge this branch before continuing with phase-details.");
  }
}
