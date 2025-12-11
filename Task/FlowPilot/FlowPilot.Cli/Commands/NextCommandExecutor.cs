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
    var (planName, shouldExit, exitCode) = _planManager.ResolvePlanName(
      args,
      msg => _logger.LogInformation(msg),
      msg => _logger.LogError(msg));

    if (shouldExit)
    {
      if (exitCode == 1)
      {
        _logger.LogInformation("Usage: flowpilot next <plan-name>");
      }

      return exitCode;
    }

    // NextCommandHandler now internally calls lint first
    await _nextHandler.ExecuteAsync(planName!);

    return 0;
  }
}
