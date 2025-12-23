using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.StateTransitions;

/// <summary>
/// Transition to the references phase.
/// </summary>
public class ReferencesTransition : IStateTransition
{
  private readonly PlanManager _planManager;
  private readonly ILogger<ReferencesTransition> _logger;

  public ReferencesTransition(PlanManager planManager, ILogger<ReferencesTransition> logger)
  {
    _planManager = planManager;
    _logger = logger;
  }

  public bool CanTransition(PlanContext context)
  {
    return !context.State.HasReferences;
  }

  public void Execute(PlanContext context)
  {
    _planManager.UpdateStateChecklist(context.PlanName, "references", true);
    _planManager.CopyTemplateToMeta(context.PlanName, "references.md");

    _logger.LogInformation("✓ Advanced to [references] phase");
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Instructions:");
    _logger.LogInformation("Update .flowpilot/plans/{PlanName}/meta/references.md", context.PlanName);
    _logger.LogInformation("Use docfork mcp server, the mslearn MCP server and web search to conduct thorough research");
    _logger.LogInformation("based on the goal stated in goal.md. Document your findings in references.md");
    _logger.LogInformation("to aid the implementation plan.");
  }
}
