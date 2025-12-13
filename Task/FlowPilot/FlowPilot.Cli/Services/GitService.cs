using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Service for Git operations.
/// </summary>
public class GitService
{
  private readonly string _repositoryPath;
  private readonly ILogger<GitService> _logger;

  public GitService(string repositoryPath, ILogger<GitService> logger)
  {
    _repositoryPath = repositoryPath;
    _logger = logger;
    _logger.LogDebug("GitService initialized with repository path: {RepositoryPath}", repositoryPath);
  }

  public List<string> GetChangedFiles()
  {
    _logger.LogDebug("GetChangedFiles called");

    using var repo = new Repository(_repositoryPath);
    var statusOptions = new StatusOptions
    {
      IncludeUntracked = true,
    };

    var status = repo.RetrieveStatus(statusOptions);
    var changedFiles = new List<string>();

    foreach (var item in status)
    {
      if (item.State != FileStatus.Ignored)
      {
        changedFiles.Add(item.FilePath);
        _logger.LogDebug("Changed file detected: {FilePath} (State: {State})", item.FilePath, item.State);
      }
    }

    _logger.LogDebug("GetChangedFiles returning {Count} changed files", changedFiles.Count);
    return changedFiles;
  }

  public string GetRepositoryRoot()
  {
    _logger.LogDebug("GetRepositoryRoot called for path: {Path}", _repositoryPath);

    try
    {
      var repoPath = Repository.Discover(_repositoryPath);

      if (string.IsNullOrEmpty(repoPath))
      {
        _logger.LogError("Repository.Discover returned null or empty for path: {Path}", _repositoryPath);
        throw new InvalidOperationException("Not a git repository");
      }

      using var repo = new Repository(repoPath);
      var workingDirectory = repo.Info.WorkingDirectory.TrimEnd('/', '\\');
      _logger.LogDebug("GetRepositoryRoot returning: {WorkingDirectory}", workingDirectory);
      return workingDirectory;
    }
    catch (RepositoryNotFoundException ex)
    {
      _logger.LogError(ex, "RepositoryNotFoundException thrown for path: {Path}", _repositoryPath);
      throw new InvalidOperationException("Not a git repository");
    }
  }

  /// <summary>
  /// Get the merge base commit SHA between the current branch and the base branch.
  /// </summary>
  /// <param name="baseBranchName">The base branch name (e.g., "origin/main")</param>
  /// <returns>The merge base commit SHA, or null if not found</returns>
  public string? GetMergeBaseSha(string baseBranchName)
  {
    _logger.LogDebug("GetMergeBaseSha called with baseBranchName: {BaseBranchName}", baseBranchName);

    using var repo = new Repository(_repositoryPath);

    // Get the current HEAD commit
    var headCommit = repo.Head.Tip;
    if (headCommit == null)
    {
      _logger.LogWarning("HEAD commit is null");
      return null;
    }

    _logger.LogDebug("HEAD commit: {HeadCommitSha}", headCommit.Sha);

    // Get the base branch
    var baseBranch = repo.Branches[baseBranchName];
    if (baseBranch == null || baseBranch.Tip == null)
    {
      _logger.LogWarning("Base branch {BaseBranchName} not found or has no tip", baseBranchName);
      return null;
    }

    _logger.LogDebug("Base branch {BaseBranchName} tip: {BaseBranchTipSha}", baseBranchName, baseBranch.Tip.Sha);

    // Find merge base
    var mergeBase = repo.ObjectDatabase.FindMergeBase(headCommit, baseBranch.Tip);
    var mergeBaseSha = mergeBase?.Sha;
    _logger.LogDebug("Merge base SHA: {MergeBaseSha}", mergeBaseSha ?? "null");
    return mergeBaseSha;
  }

  /// <summary>
  /// Get the current HEAD commit SHA.
  /// </summary>
  /// <returns>The HEAD commit SHA, or null if no HEAD</returns>
  public string? GetHeadSha()
  {
    _logger.LogDebug("GetHeadSha called");

    using var repo = new Repository(_repositoryPath);
    var headSha = repo.Head.Tip?.Sha;
    _logger.LogDebug("HEAD SHA: {HeadSha}", headSha ?? "null");
    return headSha;
  }

  /// <summary>
  /// Count the number of changed lines in a file between two commits.
  /// </summary>
  /// <param name="filePath">The relative path to the file</param>
  /// <param name="oldCommitSha">The old commit SHA</param>
  /// <param name="newCommitSha">The new commit SHA</param>
  /// <returns>The number of changed lines</returns>
  public int CountChangedLines(string filePath, string? oldCommitSha, string? newCommitSha)
  {
    _logger.LogDebug(
      "CountChangedLines called for file: {FilePath}, oldCommitSha: {OldCommitSha}, newCommitSha: {NewCommitSha}",
      filePath,
      oldCommitSha ?? "null",
      newCommitSha ?? "null");

    using var repo = new Repository(_repositoryPath);

    if (string.IsNullOrEmpty(oldCommitSha) || string.IsNullOrEmpty(newCommitSha))
    {
      _logger.LogDebug("oldCommitSha or newCommitSha is null/empty, returning 0");
      return 0;
    }

    // Look up the commits
    var oldCommit = repo.Lookup<Commit>(oldCommitSha);
    var newCommit = repo.Lookup<Commit>(newCommitSha);

    if (oldCommit == null || newCommit == null)
    {
      _logger.LogWarning(
        "Could not find commits: oldCommit={OldCommitFound}, newCommit={NewCommitFound}",
        oldCommit != null,
        newCommit != null);
      return 0;
    }

    // Normalize the file path to use forward slashes
    var normalizedPath = filePath.Replace('\\', '/');
    _logger.LogDebug("Normalized file path: {NormalizedPath}", normalizedPath);

    // Get the tree entries for the file in both commits
    var oldTree = oldCommit.Tree;
    var newTree = newCommit.Tree;

    var oldEntry = oldTree?[normalizedPath];
    var newEntry = newTree?[normalizedPath];

    // If file doesn't exist in either commit, no changes
    if (oldEntry == null && newEntry == null)
    {
      _logger.LogDebug("File doesn't exist in either commit, returning 0");
      return 0;
    }

    // Get the patch between the two versions
    var patch = repo.Diff.Compare<Patch>(oldTree, newTree, new[] { normalizedPath });

    var changedLines = CountChangedLinesInPatch(patch);
    _logger.LogDebug("CountChangedLines returning: {ChangedLines}", changedLines);
    return changedLines;
  }

  /// <summary>
  /// Count the number of changed lines in a file in staged changes (index vs HEAD).
  /// </summary>
  /// <param name="filePath">The relative path to the file</param>
  /// <returns>The number of changed lines in staged changes</returns>
  public int CountStagedChangedLines(string filePath)
  {
    _logger.LogDebug("CountStagedChangedLines called for file: {FilePath}", filePath);

    using var repo = new Repository(_repositoryPath);

    // Normalize the file path
    var normalizedPath = filePath.Replace('\\', '/');
    _logger.LogDebug("Normalized file path: {NormalizedPath}", normalizedPath);

    // Compare index (staged) to HEAD
    var patch = repo.Diff.Compare<Patch>(repo.Head.Tip?.Tree, DiffTargets.Index, new[] { normalizedPath });

    var changedLines = CountChangedLinesInPatch(patch);
    _logger.LogDebug("CountStagedChangedLines returning: {ChangedLines}", changedLines);
    return changedLines;
  }

  /// <summary>
  /// Count the number of modified lines (not additions) in a file between two commits.
  /// This counts line modifications (deletions + corresponding additions) but excludes pure additions.
  /// </summary>
  /// <param name="filePath">The relative path to the file</param>
  /// <param name="oldCommitSha">The old commit SHA</param>
  /// <param name="newCommitSha">The new commit SHA</param>
  /// <returns>The number of modified lines (deletions * 2)</returns>
  public int CountModifiedLines(string filePath, string? oldCommitSha, string? newCommitSha)
  {
    _logger.LogDebug(
      "CountModifiedLines called for file: {FilePath}, oldCommitSha: {OldCommitSha}, newCommitSha: {NewCommitSha}",
      filePath,
      oldCommitSha ?? "null",
      newCommitSha ?? "null");

    using var repo = new Repository(_repositoryPath);

    if (string.IsNullOrEmpty(oldCommitSha) || string.IsNullOrEmpty(newCommitSha))
    {
      _logger.LogDebug("oldCommitSha or newCommitSha is null/empty, returning 0");
      return 0;
    }

    // Look up the commits
    var oldCommit = repo.Lookup<Commit>(oldCommitSha);
    var newCommit = repo.Lookup<Commit>(newCommitSha);

    if (oldCommit == null || newCommit == null)
    {
      _logger.LogWarning(
        "Could not find commits: oldCommit={OldCommitFound}, newCommit={NewCommitFound}",
        oldCommit != null,
        newCommit != null);
      return 0;
    }

    // Normalize the file path to use forward slashes
    var normalizedPath = filePath.Replace('\\', '/');
    _logger.LogDebug("Normalized file path: {NormalizedPath}", normalizedPath);

    // Get the tree entries for the file in both commits
    var oldTree = oldCommit.Tree;
    var newTree = newCommit.Tree;

    var oldEntry = oldTree?[normalizedPath];
    var newEntry = newTree?[normalizedPath];

    // If file doesn't exist in either commit, no changes
    if (oldEntry == null && newEntry == null)
    {
      _logger.LogDebug("File doesn't exist in either commit, returning 0");
      return 0;
    }

    // Get the patch between the two versions
    var patch = repo.Diff.Compare<Patch>(oldTree, newTree, new[] { normalizedPath });

    var modifiedLines = CountModifiedLinesInPatch(patch);
    _logger.LogDebug("CountModifiedLines returning: {ModifiedLines}", modifiedLines);
    return modifiedLines;
  }

  /// <summary>
  /// Count the number of modified lines (not additions) in staged changes (index vs HEAD).
  /// This counts line modifications (deletions + corresponding additions) but excludes pure additions.
  /// </summary>
  /// <param name="filePath">The relative path to the file</param>
  /// <returns>The number of modified lines in staged changes</returns>
  public int CountStagedModifiedLines(string filePath)
  {
    _logger.LogDebug("CountStagedModifiedLines called for file: {FilePath}", filePath);

    using var repo = new Repository(_repositoryPath);

    // Normalize the file path
    var normalizedPath = filePath.Replace('\\', '/');
    _logger.LogDebug("Normalized file path: {NormalizedPath}", normalizedPath);

    // Compare index (staged) to HEAD
    var patch = repo.Diff.Compare<Patch>(repo.Head.Tip?.Tree, DiffTargets.Index, new[] { normalizedPath });

    var modifiedLines = CountModifiedLinesInPatch(patch);
    _logger.LogDebug("CountStagedModifiedLines returning: {ModifiedLines}", modifiedLines);
    return modifiedLines;
  }

  /// <summary>
  /// Fetch a specific remote branch.
  /// </summary>
  /// <param name="remoteName">The remote name (e.g., "origin")</param>
  /// <param name="branchName">The branch name (e.g., "main")</param>
  public void FetchRemoteBranch(string remoteName, string branchName)
  {
    if (string.IsNullOrWhiteSpace(remoteName))
    {
      throw new ArgumentException("Remote name cannot be null or empty", nameof(remoteName));
    }

    if (string.IsNullOrWhiteSpace(branchName))
    {
      throw new ArgumentException("Branch name cannot be null or empty", nameof(branchName));
    }

    _logger.LogDebug("FetchRemoteBranch called with remoteName: {RemoteName}, branchName: {BranchName}", remoteName, branchName);

    try
    {
      using var repo = new Repository(_repositoryPath);
      var remote = repo.Network.Remotes[remoteName];

      if (remote == null)
      {
        _logger.LogWarning("Remote {RemoteName} not found", remoteName);
        throw new InvalidOperationException($"Remote '{remoteName}' not found");
      }

      var refSpec = $"+refs/heads/{branchName}:refs/remotes/{remoteName}/{branchName}";
      _logger.LogDebug("Fetching with refspec: {RefSpec}", refSpec);

      LibGit2Sharp.Commands.Fetch(repo, remoteName, new[] { refSpec }, null, null);
      _logger.LogDebug("Fetch completed successfully");
    }
    catch (Exception ex) when (ex is not ArgumentException and not InvalidOperationException)
    {
      _logger.LogWarning(ex, "Failed to fetch {RemoteName}/{BranchName}", remoteName, branchName);
      throw new InvalidOperationException($"Failed to fetch branch '{branchName}' from remote '{remoteName}': {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Stage a specific file to the git index.
  /// </summary>
  /// <param name="filePath">The relative path to the file to stage</param>
  public void StageFile(string filePath)
  {
    _logger.LogDebug("StageFile called for file: {FilePath}", filePath);

    using var repo = new Repository(_repositoryPath);
    LibGit2Sharp.Commands.Stage(repo, filePath);

    _logger.LogDebug("File staged successfully: {FilePath}", filePath);
  }

  /// <summary>
  /// Reset a specific file to match HEAD, removing changes from both index and working directory.
  /// This only affects the specified file and does not touch other files.
  /// </summary>
  /// <param name="filePath">The relative path to the file to reset</param>
  public void ResetFile(string filePath)
  {
    _logger.LogDebug("ResetFile called for file: {FilePath}", filePath);

    using var repo = new Repository(_repositoryPath);

    // Normalize the file path
    var normalizedPath = filePath.Replace('\\', '/');
    _logger.LogDebug("Normalized file path: {NormalizedPath}", normalizedPath);

    // Check if file exists in HEAD
    var headCommit = repo.Head.Tip;
    if (headCommit != null)
    {
      var headTree = headCommit.Tree;
      var fileExistsInHead = headTree[normalizedPath] != null;

      _logger.LogDebug("File exists in HEAD: {FileExistsInHead}", fileExistsInHead);

      if (fileExistsInHead)
      {
        _logger.LogDebug("Resetting file to HEAD using CheckoutPaths");

        // Reset the file to HEAD (both index and working directory)
        repo.CheckoutPaths("HEAD", new[] { normalizedPath }, new CheckoutOptions
        {
          CheckoutModifiers = CheckoutModifiers.Force,
        });
        _logger.LogDebug("File reset to HEAD successfully");
      }
      else
      {
        // File doesn't exist in HEAD, just unstage and delete it
        _logger.LogDebug("File doesn't exist in HEAD, unstaging and deleting");
        try
        {
          LibGit2Sharp.Commands.Unstage(repo, normalizedPath);
          _logger.LogDebug("File unstaged successfully");
        }
        catch (Exception ex)
        {
          _logger.LogDebug(ex, "Error unstaging file (ignoring)");

          // Ignore errors if file wasn't staged
        }

        var absolutePath = Path.Combine(_repositoryPath, normalizedPath);
        if (File.Exists(absolutePath))
        {
          File.Delete(absolutePath);
          _logger.LogDebug("File deleted successfully: {AbsolutePath}", absolutePath);
        }
      }
    }
    else
    {
      _logger.LogWarning("HEAD commit is null, cannot reset file");
    }
  }

  /// <summary>
  /// Count changed lines in a patch by looking for lines starting with + or -.
  /// </summary>
  private int CountChangedLinesInPatch(Patch patch)
  {
    var changedLines = 0;
    foreach (var change in patch)
    {
      foreach (var line in change.Patch.Split('\n'))
      {
        // Count lines that start with + or - (but not +++ or ---)
        if ((line.StartsWith("+", StringComparison.Ordinal) && !line.StartsWith("+++", StringComparison.Ordinal)) ||
            (line.StartsWith("-", StringComparison.Ordinal) && !line.StartsWith("---", StringComparison.Ordinal)))
        {
          changedLines++;
        }
      }
    }

    _logger.LogDebug("CountChangedLinesInPatch counted {ChangedLines} changed lines", changedLines);
    return changedLines;
  }

  private int CountModifiedLinesInPatch(Patch patch)
  {
    // Count only deletions, then multiply by 2 to account for the corresponding additions
    // This excludes pure additions (new lines) from the count
    var deletions = 0;
    foreach (var change in patch)
    {
      foreach (var line in change.Patch.Split('\n'))
      {
        // Count lines that start with - (but not ---)
        if (line.StartsWith("-", StringComparison.Ordinal) && !line.StartsWith("---", StringComparison.Ordinal))
        {
          deletions++;
        }
      }
    }

    var modifiedLines = deletions * 2;
    _logger.LogDebug(
      "CountModifiedLinesInPatch counted {Deletions} deletions, returning {ModifiedLines} modified lines",
      deletions,
      modifiedLines);

    // Each deletion has a corresponding addition (the modified line)
    return modifiedLines;
  }
}
