using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.StateTransitions;

/// <summary>
/// Transition to the system-analysis phase.
/// </summary>
public class SystemAnalysisTransition : IStateTransition
{
  private readonly PlanManager _planManager;
  private readonly ILogger<SystemAnalysisTransition> _logger;

  public SystemAnalysisTransition(PlanManager planManager, ILogger<SystemAnalysisTransition> logger)
  {
    _planManager = planManager;
    _logger = logger;
  }

  public bool CanTransition(PlanContext context)
  {
    return context.State.HasReferences && !context.State.HasSystemAnalysis;
  }

  public void Execute(PlanContext context)
  {
    _planManager.UpdateStateChecklist(context.PlanName, "system-analysis", true);
    _planManager.CopyTemplateToMeta(context.PlanName, "system-analysis.md");

    _logger.LogInformation("✓ Advanced to [system-analysis] phase");
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Instructions:");
    _logger.LogInformation("Update .flowpilot/plans/{PlanName}/meta/system-analysis.md", context.PlanName);
    _logger.LogInformation("Use RoslynMCP mcp server (among other techniques) to analyze the current parts of the system that are relevant to the goal");
    _logger.LogInformation("stated in goal.md and record them in system-analysis.md.");
  }
}
