using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the lint command.
/// </summary>
public class LintCommandExecutor : ICommand
{
  private readonly PlanManager _planManager;
  private readonly GitService _gitService;
  private readonly LintCommandHandler _lintHandler;

  public LintCommandExecutor(
    PlanManager planManager,
    GitService gitService,
    LintCommandHandler lintHandler)
  {
    _planManager = planManager;
    _gitService = gitService;
    _lintHandler = lintHandler;
  }

  public string Name => "lint";

  public string Description => "Validate the plan follows FlowPilot rules";

  public async Task<int> ExecuteAsync(string[] args)
  {
    if (args.Length == 0)
    {
      Console.WriteLine("Error: Plan name is required.");
      Console.WriteLine("Usage: flowpilot lint <plan-name>");
      return 1;
    }

    var planName = args[0];

    if (!_planManager.PlanExists(planName))
    {
      Console.WriteLine($"Error: Plan '{planName}' not found.");
      return 1;
    }

    try
    {
      _gitService.GetRepositoryRoot();
    }
    catch (InvalidOperationException)
    {
      Console.WriteLine("Error: Not in a git repository. FlowPilot lint requires git.");
      return 1;
    }

    var exitCode = await _lintHandler.ExecuteAsync(planName);
    return exitCode;
  }
}
