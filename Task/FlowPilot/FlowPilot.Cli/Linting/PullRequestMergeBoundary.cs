using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

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
  private readonly ILogger<PullRequestMergeBoundary> _logger;

  public PullRequestMergeBoundary(GitService gitService, IFileSystemService fileSystem, ILogger<PullRequestMergeBoundary> logger)
  {
    _gitService = gitService;
    _fileSystem = fileSystem;
    _logger = logger;
  }

  public Task ExecuteAsync(PlanContext context)
  {
    // Get the relative path to state.md
    var relativeStatePath = Path.GetRelativePath(context.RepositoryRoot, context.StateFilePath);

    // Try to find merge-base with common base branches
    var baseBranches = new[] { "origin/main", "origin/master", "main", "master" };
    string? mergeBaseSha = null;
    string? foundBaseBranch = null;

    foreach (var baseBranch in baseBranches)
    {
      try
      {
        mergeBaseSha = _gitService.GetMergeBaseSha(baseBranch);
        if (mergeBaseSha != null)
        {
          foundBaseBranch = baseBranch;
          break;
        }
      }
      catch
      {
        // Continue to try next base branch
      }
    }

    // If we can't find a merge-base, try to fetch the missing branch
    if (mergeBaseSha == null)
    {
      _logger.LogDebug("No merge-base found with any of the standard branches, attempting to fetch");

      // Try to fetch each base branch directly
      foreach (var baseBranch in baseBranches)
      {
        // Only try to fetch origin/* branches (remote branches)
        if (!baseBranch.StartsWith("origin/", StringComparison.Ordinal))
        {
          continue;
        }

        // Parse remote and branch name (expecting format "remote/branch")
        var parts = baseBranch.Split('/', 2);
        if (parts.Length != 2)
        {
          _logger.LogDebug("Unexpected branch format: {BaseBranch}, expected 'remote/branch'", baseBranch);
          continue;
        }

        var remoteName = parts[0];
        var branchName = parts[1];

        try
        {
          _logger.LogDebug("Attempting to fetch {RemoteName}/{BranchName}", remoteName, branchName);
          _gitService.FetchRemoteBranch(remoteName, branchName);
          _logger.LogWarning(
            "Fetched missing base branch {BaseBranch}. " +
            "Consider updating .github/workflows/copilot-setup-steps.yml to checkout this branch to avoid this fetch in the future.",
            baseBranch);

          // Try to get merge-base again after fetching
          mergeBaseSha = _gitService.GetMergeBaseSha(baseBranch);
          if (mergeBaseSha != null)
          {
            foundBaseBranch = baseBranch;
            break;
          }
        }
        catch (Exception ex)
        {
          _logger.LogDebug(ex, "Failed to fetch branch {BaseBranch}", baseBranch);
        }
      }

      // If still no merge-base found, skip this rule
      if (mergeBaseSha == null)
      {
        _logger.LogDebug("Unable to find merge-base even after attempting fetch, skipping merge boundary check");
        return Task.CompletedTask;
      }
    }

    // Get current HEAD SHA using GitService
    var headSha = _gitService.GetHeadSha();

    // Count changed lines (modified, not added or deleted)
    // We only care about modifications (checkbox state changes), not additions (new checkboxes)
    var committedModifications = _gitService.CountModifiedLines(relativeStatePath, mergeBaseSha, headSha);
    var stagedModifications = _gitService.CountStagedModifiedLines(relativeStatePath);

    // Total modifications (checkbox state changes)
    var totalModifications = committedModifications + stagedModifications;

    // A single checkbox change results in 2 line modifications: one deletion ([ ]) and one addition ([x])
    // So we allow at most 2 modifications (1 checkbox state change)
    // Note: Adding new checkboxes (new lines) doesn't count toward this limit
    const int maxAllowedModifications = 2;

    if (totalModifications > maxAllowedModifications)
    {
      context.LintingErrors.Add(
        "Unable to proceed to the next phase. You have reached a pull request merge boundary. " +
        "You must stop work and allow the pull request to be reviewed and merged. " +
        "If your current phase has any #Verification conditions, make sure they are all passing before stopping work.");
    }

    return Task.CompletedTask;
  }
}
