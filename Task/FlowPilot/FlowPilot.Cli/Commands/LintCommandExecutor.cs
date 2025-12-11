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
    var (planName, shouldExit, exitCode) = _planManager.ResolvePlanName(
      args,
      msg => _logger.LogInformation(msg),
      msg => _logger.LogError(msg));

    if (shouldExit)
    {
      if (exitCode == 1)
      {
        _logger.LogInformation("Usage: flowpilot lint <plan-name>");
      }

      return exitCode;
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

    var lintExitCode = await _lintHandler.ExecuteAsync(planName!);
    return lintExitCode;
  }
}
