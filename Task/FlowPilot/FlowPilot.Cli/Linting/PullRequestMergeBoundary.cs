using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that at most two lines in state.md have changed (one checkbox change)
/// by comparing the merge-base against staged changes and already committed changes.
/// This enforces the branch-per-phase workflow.
/// </summary>
public class PullRequestMergeBoundary : ILintingRule
{
  private readonly GitService _gitService;
  private readonly IFileSystemService _fileSystem;

  public PullRequestMergeBoundary(GitService gitService, IFileSystemService fileSystem)
  {
    _gitService = gitService;
    _fileSystem = fileSystem;
  }

  public Task ExecuteAsync(PlanContext context)
  {
    // Get the relative path to state.md
    var relativeStatePath = Path.GetRelativePath(context.RepositoryRoot, context.StateFilePath);

    // Try to find merge-base with common base branches
    var baseBranches = new[] { "origin/main", "origin/master", "main", "master" };
    string? mergeBaseSha = null;

    foreach (var baseBranch in baseBranches)
    {
      try
      {
        mergeBaseSha = _gitService.GetMergeBaseSha(baseBranch);
        if (mergeBaseSha != null)
        {
          break;
        }
      }
      catch
      {
        // Continue to try next base branch
      }
    }

    // If we can't find a merge-base, skip this rule (e.g., first commit, or detached HEAD)
    if (mergeBaseSha == null)
    {
      return Task.CompletedTask;
    }

    // Get current HEAD SHA using GitService
    var headSha = _gitService.GetHeadSha();

    // Count changes between merge-base and HEAD (committed changes on this branch)
    var committedChanges = _gitService.CountChangedLines(relativeStatePath, mergeBaseSha, headSha);

    // Count changes in staged area (index vs HEAD)
    var stagedChanges = _gitService.CountStagedChangedLines(relativeStatePath);

    // Total changes
    var totalChanges = committedChanges + stagedChanges;

    // A single checkbox change results in 2 line changes: one deletion ([ ]) and one addition ([x])
    // So we allow at most 2 total changes
    const int maxAllowedChanges = 2;

    if (totalChanges > maxAllowedChanges)
    {
      context.LintingErrors.Add(
        "Unable to proceed to the next phase. You have reached a pull request merge boundary. " +
        "You must stop work and allow the pull request to be reviewed and merged. " +
        "If your current phase has any #Verification conditions, make sure they are all passing before stopping work.");
    }

    return Task.CompletedTask;
  }
}
