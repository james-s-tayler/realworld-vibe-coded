using FlowPilot.Cli.Models;
using FlowPilot.Cli.StateTransitions;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Handles the logic for the 'next' command using discrete state transitions.
/// </summary>
public class NextCommandHandler
{
  private readonly PlanManager _planManager;
  private readonly IEnumerable<IStateTransition> _stateTransitions;
  private readonly ILogger<NextCommandHandler> _logger;

  public NextCommandHandler(
    PlanManager planManager,
    IEnumerable<IStateTransition> stateTransitions,
    ILogger<NextCommandHandler> logger)
  {
    _planManager = planManager;
    _stateTransitions = stateTransitions;
    _logger = logger;
  }

  public void Execute(string planName)
  {
    var state = _planManager.GetCurrentState(planName);

    if (!state.IsInitialized)
    {
      _logger.LogError("Plan not initialized. Run 'flowpilot new' first");
      Environment.Exit(1);
      return;
    }

    // Build plan context
    var context = new PlanContext
    {
      PlanName = planName,
      State = state,
      PlanDirectory = _planManager.GetPlanDirectory(planName),
      MetaDirectory = _planManager.GetMetaDirectory(planName),
      PlanSubDirectory = _planManager.GetPlanSubDirectory(planName),
      StateFilePath = _planManager.GetStateFilePath(planName),
    };

    // Find and execute the first applicable transition
    var transition = _stateTransitions.FirstOrDefault(t => t.CanTransition(context));

    if (transition != null)
    {
      transition.Execute(context);
    }
    else
    {
      _logger.LogError("No applicable state transition found");
      Environment.Exit(1);
    }
  }
}
