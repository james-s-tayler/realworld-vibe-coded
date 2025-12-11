using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.StateTransitions;

/// <summary>
/// Transition to the key-decisions phase.
/// </summary>
public class KeyDecisionsTransition : IStateTransition
{
  private readonly PlanManager _planManager;
  private readonly ILogger<KeyDecisionsTransition> _logger;

  public KeyDecisionsTransition(PlanManager planManager, ILogger<KeyDecisionsTransition> logger)
  {
    _planManager = planManager;
    _logger = logger;
  }

  public bool CanTransition(PlanContext context)
  {
    return context.State.HasSystemAnalysis && !context.State.HasKeyDecisions;
  }

  public void Execute(PlanContext context)
  {
    _planManager.UpdateStateChecklist(context.PlanName, "key-decisions", true);
    _planManager.CopyTemplateToMeta(context.PlanName, "key-decisions.md");

    _logger.LogInformation("✓ Advanced to [key-decisions] phase");
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Instructions:");
    _logger.LogInformation("Update .flowpilot/plans/{PlanName}/meta/key-decisions.md", context.PlanName);
    _logger.LogInformation("Document any decisions that need to be made based on the contents of");
    _logger.LogInformation("goal.md, references.md, and system-analysis.md.");
    _logger.LogInformation(string.Empty);
    _logger.LogWarning("A new branch is required to proceed past key-decisions.");
    _logger.LogInformation("After committing, merge this branch before continuing with phase-analysis.");
  }
}
