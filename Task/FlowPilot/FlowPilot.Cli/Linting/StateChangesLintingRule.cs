using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that only one state.md change is made at a time.
/// </summary>
public class StateChangesLintingRule : ILintingRule
{
  private readonly GitService _gitService;

  public StateChangesLintingRule(GitService gitService)
  {
    _gitService = gitService;
  }

  public Task<List<string>> ExecuteAsync(PlanContext context)
  {
    var errors = new List<string>();

    var changedFiles = _gitService.GetChangedFiles();
    var repoRoot = _gitService.GetRepositoryRoot();

    // Normalize paths
    var relativeStatePath = Path.GetRelativePath(repoRoot, context.StateFilePath);

    // Count how many times state.md appears in changed files (using cross-platform path comparison)
    var normalizedStatePath = relativeStatePath.Replace('\\', '/');
    var stateChanges = changedFiles.Count(f => f.Replace('\\', '/').Equals(normalizedStatePath, StringComparison.OrdinalIgnoreCase));

    if (stateChanges > 1)
    {
      errors.Add($"Multiple state.md changes detected ({stateChanges}). Only one state transition is allowed per commit.");
    }

    return Task.FromResult(errors);
  }
}
