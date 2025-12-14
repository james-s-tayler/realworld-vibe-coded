using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the refine command.
/// </summary>
public class UnstuckCommand : ICommand
{
  private readonly PlanManager _planManager;
  private readonly ILogger<UnstuckCommand> _logger;

  public UnstuckCommand(ILogger<UnstuckCommand> logger, PlanManager planManager)
  {
    _logger = logger;
    _planManager = planManager;
  }

  public string Name => "unstuck";

  public string Description => "Receive guidance from the user about how to proceed when stuck.";

  public Task<int> ExecuteAsync(string[] args)
  {
    var (planName, shouldExit, exitCode) = _planManager.ResolvePlanName(
      args,
      msg => _logger.LogInformation(msg),
      msg => _logger.LogError(msg));

    if (shouldExit)
    {
      if (exitCode == 1)
      {
        _logger.LogInformation("Usage: flowpilot unstuck");
      }

      return Task.FromResult(exitCode);
    }

    // Get current state to find the active phase
    var state = _planManager.GetCurrentState(planName!);

    if (!state.IsInitialized)
    {
      _logger.LogError("Plan not initialized. Run 'flowpilot new' first");
      return Task.FromResult(1);
    }

    var currentPhase = state.Phases.LastOrDefault(p => p.IsChecked);

    // If all phases are complete, check the last phase
    if (currentPhase == null && state.Phases.Any())
    {
      currentPhase = state.Phases.Last();
      _logger.LogInformation("All phases are complete. Checking verification for the last phase.");
    }

    if (currentPhase == null)
    {
      // Check if we're still in planning stages
      if (!state.HasPhaseAnalysis)
      {
        _logger.LogInformation("No phases defined yet. Complete the planning stages first.");
        return Task.FromResult(0);
      }

      _logger.LogError("No phases found in the plan.");
      return Task.FromResult(1);
    }

    _logger.LogInformation($"It looks like you're stuck on phase {currentPhase.PhaseNumber} and unable to carry out the plan successfully as described.");
    _logger.LogInformation($"The user has approved the selected option in .flowpilot/plans/{planName}/meta/phase-{currentPhase.PhaseNumber}-stuck-analysis.md as the way forward.");
    _logger.LogInformation($"Update .flowpilot/plans/{planName}/meta/phase-analysis.md and .flowpilot/plans/{planName}/plan/phase-{currentPhase.PhaseNumber}-details.md to reflect this.");
    _logger.LogInformation("Afterwards, please proceed to carry out the updated plan now that you are unstuck.");

    return Task.FromResult(0);
  }
}
