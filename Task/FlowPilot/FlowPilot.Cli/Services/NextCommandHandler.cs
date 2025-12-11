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

    // Check for hard boundary after key-decisions
    if (state.HasKeyDecisions && !state.HasPhaseAnalysis)
    {
      _logger.LogWarning("Hard boundary reached after key-decisions phase");
      _logger.LogInformation(string.Empty);
      _logger.LogInformation("Nothing more can be done on this branch.");
      _logger.LogInformation("To proceed to phase-analysis, you must:");
      _logger.LogInformation("  1. Merge this branch");
      _logger.LogInformation("  2. Create a new branch");
      _logger.LogInformation("  3. Run 'flowpilot next {PlanName}' again", planName);
      _logger.LogInformation(string.Empty);
      Environment.Exit(0);
      return;
    }

    // Check for hard boundary after phase-details
    if (state.HasPhaseDetails && !state.Phases.Any(p => p.IsComplete))
    {
      _logger.LogWarning("Hard boundary reached after phase-details");
      _logger.LogInformation(string.Empty);
      _logger.LogInformation("Nothing more can be done on this branch.");
      _logger.LogInformation("To proceed to phase implementation, you must:");
      _logger.LogInformation("  1. Merge this branch");
      _logger.LogInformation("  2. Create a new branch for the first phase");
      _logger.LogInformation("  3. Run 'flowpilot next {PlanName}' again", planName);
      _logger.LogInformation(string.Empty);
      Environment.Exit(0);
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
      // Check if plan is complete
      if (state.Phases.Any() && state.Phases.All(p => p.IsComplete))
      {
        _logger.LogInformation("✓ Plan '{PlanName}' is complete!", planName);
        _logger.LogInformation("All phases have been finished.");
      }
      else
      {
        _logger.LogError("No applicable state transition found");
        Environment.Exit(1);
      }
    }
  }
}
