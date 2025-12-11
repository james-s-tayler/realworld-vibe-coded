using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the next command.
/// </summary>
public class NextCommandExecutor : ICommand
{
  private readonly PlanManager _planManager;
  private readonly NextCommandHandler _nextHandler;
  private readonly ILogger<NextCommandExecutor> _logger;

  public NextCommandExecutor(
    PlanManager planManager,
    NextCommandHandler nextHandler,
    ILogger<NextCommandExecutor> logger)
  {
    _planManager = planManager;
    _nextHandler = nextHandler;
    _logger = logger;
  }

  public string Name => "next";

  public string Description => "Advance to the next phase of the plan";

  public async Task<int> ExecuteAsync(string[] args)
  {
    // Get all available plans
    var allPlans = _planManager.GetAllPlans();

    // If no plans exist, exit successfully
    if (allPlans.Count == 0)
    {
      _logger.LogInformation("No current plans. Successful exit.");
      return 0;
    }

    string planName;

    // If no argument provided, try to use the default plan
    if (args.Length == 0)
    {
      // If there's only one plan, use it as default
      if (allPlans.Count == 1)
      {
        planName = allPlans[0];
        _logger.LogInformation("Using default plan: {PlanName}", planName);
      }
      else
      {
        // Multiple plans exist, require explicit plan name
        _logger.LogError("Multiple plans exist. Please specify a plan name.");
        _logger.LogInformation("Available plans: {Plans}", string.Join(", ", allPlans));
        _logger.LogInformation("Usage: flowpilot next <plan-name>");
        return 1;
      }
    }
    else
    {
      planName = args[0];

      if (!_planManager.PlanExists(planName))
      {
        _logger.LogError("Plan '{PlanName}' not found. Run 'flowpilot new {PlanName2}' first", planName, planName);
        return 1;
      }
    }

    // NextCommandHandler now internally calls lint first
    await _nextHandler.ExecuteAsync(planName);

    return 0;
  }
}
