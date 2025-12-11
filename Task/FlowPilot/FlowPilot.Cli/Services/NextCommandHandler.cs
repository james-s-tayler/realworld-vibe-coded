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
  private readonly LintCommandHandler _lintCommandHandler;
  private readonly IEnumerable<IStateTransition> _stateTransitions;
  private readonly ILogger<NextCommandHandler> _logger;

  public NextCommandHandler(
    PlanManager planManager,
    LintCommandHandler lintCommandHandler,
    IEnumerable<IStateTransition> stateTransitions,
    ILogger<NextCommandHandler> logger)
  {
    _planManager = planManager;
    _lintCommandHandler = lintCommandHandler;
    _stateTransitions = stateTransitions;
    _logger = logger;
  }

  public async Task ExecuteAsync(string planName)
  {
    _logger.LogDebug("NextCommandHandler.ExecuteAsync called for plan '{PlanName}'", planName);

    // First, run lint to validate current state
    _logger.LogInformation("Running lint validation...");
    var lintResult = await _lintCommandHandler.ExecuteAsync(planName);
    _logger.LogDebug("Lint result: {LintResult}", lintResult);

    if (lintResult != 0)
    {
      _logger.LogError("Lint failed. Fix the errors before proceeding.");
      Environment.Exit(1);
      return;
    }

    _logger.LogInformation(string.Empty);

    var state = _planManager.GetCurrentState(planName);
    _logger.LogDebug(
      "Current state: HasPhaseAnalysis={HasPhaseAnalysis}, HasPhaseDetails={HasPhaseDetails}",
      state.HasPhaseAnalysis,
      state.HasPhaseDetails);

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
    _logger.LogDebug("Searching for applicable state transition among {Count} transitions", _stateTransitions.Count());
    var transition = _stateTransitions.FirstOrDefault(t =>
    {
      var canTransition = t.CanTransition(context);
      _logger.LogDebug(
        "Transition {TransitionType} CanTransition: {CanTransition}",
        t.GetType().Name,
        canTransition);
      return canTransition;
    });

    if (transition != null)
    {
      _logger.LogDebug("Executing transition: {TransitionType}", transition.GetType().Name);
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
