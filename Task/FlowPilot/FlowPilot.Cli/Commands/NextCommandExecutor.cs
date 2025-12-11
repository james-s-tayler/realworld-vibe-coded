using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the next command.
/// </summary>
public class NextCommandExecutor : ICommand
{
  private readonly PlanManager _planManager;
  private readonly GitService _gitService;
  private readonly LintCommandHandler _lintHandler;
  private readonly NextCommandHandler _nextHandler;

  public NextCommandExecutor(
    PlanManager planManager,
    GitService gitService,
    LintCommandHandler lintHandler,
    NextCommandHandler nextHandler)
  {
    _planManager = planManager;
    _gitService = gitService;
    _lintHandler = lintHandler;
    _nextHandler = nextHandler;
  }

  public string Name => "next";

  public string Description => "Advance to the next phase of the plan";

  public async Task<int> ExecuteAsync(string[] args)
  {
    if (args.Length == 0)
    {
      Console.WriteLine("Error: Plan name is required.");
      Console.WriteLine("Usage: flowpilot next <plan-name>");
      return 1;
    }

    var planName = args[0];

    if (!_planManager.PlanExists(planName))
    {
      Console.WriteLine($"Error: Plan '{planName}' not found. Run 'flowpilot init {planName}' first.");
      return 1;
    }

    // Run lint first
    try
    {
      _gitService.GetRepositoryRoot();

      var lintResult = await _lintHandler.ExecuteAsync(planName);

      if (lintResult != 0)
      {
        Console.WriteLine();
        Console.WriteLine("❌ Cannot proceed - lint check failed. Fix the issues above first.");
        return 1;
      }
    }
    catch (InvalidOperationException)
    {
      // Not in git repo - continue without lint (for testing purposes)
      Console.WriteLine("⚠️  Warning: Not in a git repository. Skipping lint check.");
    }

    _nextHandler.Execute(planName);

    return 0;
  }
}
