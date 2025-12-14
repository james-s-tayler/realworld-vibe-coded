using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the refine command.
/// </summary>
public class StuckCommand : ICommand
{
  private readonly PlanManager _planManager;
  private readonly ILogger<StuckCommand> _logger;

  public StuckCommand(ILogger<StuckCommand> logger, PlanManager planManager)
  {
    _logger = logger;
    _planManager = planManager;
  }

  public string Name => "stuck";

  public string Description => "Analyze unsuccessful implementation attempts and present options to the user for guidance.";

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
        _logger.LogInformation("Usage: flowpilot verify <plan-name>");
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
    _logger.LogInformation("The user has request you do some analysis to help you both understand the challenges and refine the plan together.");
    _logger.LogInformation("Based on your understanding of the challenges, present a series of options in the .flowpilot/template/key-decisions.md format " +
                           $"saved to .flowpilot/plans/{planName}/meta/phase-{currentPhase.PhaseNumber}-stuck-analysis.md and report them to the user for guidance.");

    return Task.FromResult(0);
  }
}
