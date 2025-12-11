using FlowPilot.Cli.Linting;
using FlowPilot.Cli.Models;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Handles the logic for the 'lint' command using discrete linting rules.
/// </summary>
public class LintCommandHandler
{
  private readonly PlanManager _planManager;
  private readonly GitService _gitService;
  private readonly IEnumerable<ILintingRule> _lintingRules;

  public LintCommandHandler(
    PlanManager planManager,
    GitService gitService,
    IEnumerable<ILintingRule> lintingRules)
  {
    _planManager = planManager;
    _gitService = gitService;
    _lintingRules = lintingRules;
  }

  public async Task<int> ExecuteAsync(string planName)
  {
    // Build plan context
    var state = _planManager.GetCurrentState(planName);

    if (!state.IsInitialized)
    {
      var errors = new List<string> { "Plan not initialized" };
      PrintErrors(errors);
      return 1;
    }

    var context = new PlanContext
    {
      PlanName = planName,
      State = state,
      PlanDirectory = _planManager.GetPlanDirectory(planName),
      MetaDirectory = _planManager.GetMetaDirectory(planName),
      PlanSubDirectory = _planManager.GetPlanSubDirectory(planName),
      StateFilePath = _planManager.GetStateFilePath(planName),
      RepositoryRoot = _gitService.GetRepositoryRoot(),
      ChangedFiles = _gitService.GetChangedFiles(),
      LintingErrors = new List<string>(),
    };

    // Execute all linting rules
    foreach (var rule in _lintingRules)
    {
      await rule.ExecuteAsync(context);
    }

    if (context.LintingErrors.Count > 0)
    {
      PrintErrors(context.LintingErrors);
      return 1;
    }

    Console.WriteLine("✓ Lint passed - plan follows FlowPilot rules");
    return 0;
  }

  private void PrintErrors(List<string> errors)
  {
    Console.WriteLine("❌ Lint failed with the following errors:");
    Console.WriteLine();

    foreach (var error in errors)
    {
      Console.WriteLine($"  • {error}");
    }

    Console.WriteLine();
    Console.WriteLine("Fix these issues and try again.");
  }
}
