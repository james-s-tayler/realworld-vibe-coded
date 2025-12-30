using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the new command.
/// </summary>
public class NewCommandHandler : ICommand
{
  private readonly PlanManager _planManager;
  private readonly ILogger<NewCommandHandler> _logger;

  public NewCommandHandler(
    PlanManager planManager,
    ILogger<NewCommandHandler> logger)
  {
    _planManager = planManager;
    _logger = logger;
  }

  public string Name => "new";

  public string Description => "Create a new FlowPilot plan";

  public Task<int> ExecuteAsync(string[] args)
  {
    if (args.Length == 0)
    {
      _logger.LogError("Plan name is required");
      _logger.LogInformation("Usage: flowpilot new <plan-name>");
      return Task.FromResult(1);
    }

    var planName = args[0];
    _logger.LogInformation("Creating FlowPilot plan: {PlanName}", planName);

    if (_planManager.PlanExists(planName))
    {
      _logger.LogError("Plan '{PlanName}' already exists", planName);
      return Task.FromResult(1);
    }

    _planManager.InitializePlan(planName);

    _logger.LogInformation("✓ Plan '{PlanName}' created successfully", planName);
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Next steps:");
    _logger.LogInformation("1. Update .flowpilot/plans/{PlanName}/meta/goal.md with your feature requirements", planName);
    _logger.LogInformation("2. Commit this change to your repository");
    _logger.LogInformation("3. Run 'flowpilot next {PlanName}' to continue", planName);

    return Task.FromResult(0);
  }
}
