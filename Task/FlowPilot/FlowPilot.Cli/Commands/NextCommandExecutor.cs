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
    if (args.Length == 0)
    {
      _logger.LogError("Plan name is required");
      _logger.LogInformation("Usage: flowpilot next <plan-name>");
      return 1;
    }

    var planName = args[0];

    if (!_planManager.PlanExists(planName))
    {
      _logger.LogError("Plan '{PlanName}' not found. Run 'flowpilot new {PlanName2}' first", planName, planName);
      return 1;
    }

    // NextCommandHandler now internally calls lint first
    await _nextHandler.ExecuteAsync(planName);

    return 0;
  }
}
