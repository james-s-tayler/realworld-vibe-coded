using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the lint command.
/// </summary>
public class LintCommandExecutor : ICommand
{
  private readonly PlanManager _planManager;
  private readonly GitService _gitService;
  private readonly LintCommandHandler _lintHandler;
  private readonly ILogger<LintCommandExecutor> _logger;

  public LintCommandExecutor(
    PlanManager planManager,
    GitService gitService,
    LintCommandHandler lintHandler,
    ILogger<LintCommandExecutor> logger)
  {
    _planManager = planManager;
    _gitService = gitService;
    _lintHandler = lintHandler;
    _logger = logger;
  }

  public string Name => "lint";

  public string Description => "Validate the plan follows FlowPilot rules";

  public async Task<int> ExecuteAsync(string[] args)
  {
    if (args.Length == 0)
    {
      _logger.LogError("Plan name is required");
      _logger.LogInformation("Usage: flowpilot lint <plan-name>");
      return 1;
    }

    var planName = args[0];

    if (!_planManager.PlanExists(planName))
    {
      _logger.LogError("Plan '{PlanName}' not found", planName);
      return 1;
    }

    try
    {
      _gitService.GetRepositoryRoot();
    }
    catch (InvalidOperationException)
    {
      _logger.LogError("Not in a git repository. FlowPilot lint requires git");
      return 1;
    }

    var exitCode = await _lintHandler.ExecuteAsync(planName);
    return exitCode;
  }
}
