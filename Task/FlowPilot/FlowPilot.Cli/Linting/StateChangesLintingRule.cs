using FlowPilot.Cli.Models;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that only one state.md change is made at a time.
/// </summary>
public class StateChangesLintingRule : ILintingRule
{
  public Task ExecuteAsync(PlanContext context)
  {
    // Normalize paths
    var relativeStatePath = Path.GetRelativePath(context.RepositoryRoot, context.StateFilePath);

    // Count how many times state.md appears in changed files (using cross-platform path comparison)
    var normalizedStatePath = relativeStatePath.Replace('\\', '/');
    var stateChanges = context.ChangedFiles.Count(f => f.Replace('\\', '/').Equals(normalizedStatePath, StringComparison.OrdinalIgnoreCase));

    if (stateChanges > 1)
    {
      context.LintingErrors.Add($"Multiple state.md changes detected ({stateChanges}). Only one state transition is allowed per commit.");
    }

    return Task.CompletedTask;
  }
}
