using FlowPilot.Cli.Linting;
using FlowPilot.Cli.Models;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Handles the logic for the 'lint' command using discrete linting rules.
/// </summary>
public class LintCommandHandler
{
  private readonly PlanManager _planManager;
  private readonly GitService _gitService;
  private readonly IEnumerable<ILintingRule> _lintingRules;
  private readonly ILogger<LintCommandHandler> _logger;

  public LintCommandHandler(
    PlanManager planManager,
    GitService gitService,
    IEnumerable<ILintingRule> lintingRules,
    ILogger<LintCommandHandler> logger)
  {
    _planManager = planManager;
    _gitService = gitService;
    _lintingRules = lintingRules;
    _logger = logger;
  }

  public async Task<int> ExecuteAsync(string planName)
  {
    _logger.LogInformation("LintCommandHandler.ExecuteAsync called for plan: {PlanName}", planName);

    // Build plan context
    var state = _planManager.GetCurrentState(planName);

    _logger.LogDebug(
      "Plan state loaded: IsInitialized={IsInitialized}, PhaseCount={PhaseCount}",
      state.IsInitialized,
      state.Phases.Count);

    if (!state.IsInitialized)
    {
      var errors = new List<string> { "Plan not initialized" };
      _logger.LogError("Plan not initialized: {PlanName}", planName);
      PrintErrors(errors);
      return 1;
    }

    _logger.LogDebug("Building plan context");
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

    _logger.LogDebug(
      "Plan context built: RepositoryRoot={RepositoryRoot}, ChangedFilesCount={ChangedFilesCount}",
      context.RepositoryRoot,
      context.ChangedFiles.Count);

    // Execute all linting rules
    _logger.LogInformation("Executing {RuleCount} linting rules", _lintingRules.Count());
    var ruleIndex = 0;
    foreach (var rule in _lintingRules)
    {
      ruleIndex++;
      var ruleName = rule.GetType().Name;
      _logger.LogDebug(
        "Executing linting rule {RuleIndex}/{RuleCount}: {RuleName}",
        ruleIndex,
        _lintingRules.Count(),
        ruleName);

      await rule.ExecuteAsync(context);

      if (context.LintingErrors.Count > 0)
      {
        _logger.LogDebug("Linting rule {RuleName} added {ErrorCount} errors", ruleName, context.LintingErrors.Count);
      }
      else
      {
        _logger.LogDebug("Linting rule {RuleName} passed", ruleName);
      }
    }

    _logger.LogInformation("All linting rules executed. Total errors: {ErrorCount}", context.LintingErrors.Count);

    if (context.LintingErrors.Count > 0)
    {
      PrintErrors(context.LintingErrors);
      return 1;
    }

    _logger.LogInformation("✓ Lint passed - plan follows FlowPilot rules");
    return 0;
  }

  private void PrintErrors(List<string> errors)
  {
    _logger.LogError("Lint failed with the following errors:");
    _logger.LogInformation(string.Empty);

    foreach (var error in errors)
    {
      _logger.LogInformation("  • {Error}", error);
    }

    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Fix these issues and try again.");
  }
}
