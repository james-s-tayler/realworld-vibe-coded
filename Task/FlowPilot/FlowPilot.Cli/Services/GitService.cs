using LibGit2Sharp;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Service for Git operations.
/// </summary>
public class GitService
{
  private readonly string _repositoryPath;

  public GitService(string repositoryPath)
  {
    _repositoryPath = repositoryPath;
  }

  public List<string> GetChangedFiles()
  {
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
      }
    }

    return changedFiles;
  }

  public string GetRepositoryRoot()
  {
    try
    {
      var repoPath = Repository.Discover(_repositoryPath);

      if (string.IsNullOrEmpty(repoPath))
      {
        throw new InvalidOperationException("Not a git repository");
      }

      using var repo = new Repository(repoPath);
      return repo.Info.WorkingDirectory.TrimEnd('/', '\\');
    }
    catch (RepositoryNotFoundException)
    {
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
    using var repo = new Repository(_repositoryPath);

    // Get the current HEAD commit
    var headCommit = repo.Head.Tip;
    if (headCommit == null)
    {
      return null;
    }

    // Get the base branch
    var baseBranch = repo.Branches[baseBranchName];
    if (baseBranch == null || baseBranch.Tip == null)
    {
      return null;
    }

    // Find merge base
    var mergeBase = repo.ObjectDatabase.FindMergeBase(headCommit, baseBranch.Tip);
    return mergeBase?.Sha;
  }

  /// <summary>
  /// Get the current HEAD commit SHA.
  /// </summary>
  /// <returns>The HEAD commit SHA, or null if no HEAD</returns>
  public string? GetHeadSha()
  {
    using var repo = new Repository(_repositoryPath);
    return repo.Head.Tip?.Sha;
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
    using var repo = new Repository(_repositoryPath);

    if (string.IsNullOrEmpty(oldCommitSha) || string.IsNullOrEmpty(newCommitSha))
    {
      return 0;
    }

    // Look up the commits
    var oldCommit = repo.Lookup<Commit>(oldCommitSha);
    var newCommit = repo.Lookup<Commit>(newCommitSha);

    if (oldCommit == null || newCommit == null)
    {
      return 0;
    }

    // Normalize the file path to use forward slashes
    var normalizedPath = filePath.Replace('\\', '/');

    // Get the tree entries for the file in both commits
    var oldTree = oldCommit.Tree;
    var newTree = newCommit.Tree;

    var oldEntry = oldTree?[normalizedPath];
    var newEntry = newTree?[normalizedPath];

    // If file doesn't exist in either commit, no changes
    if (oldEntry == null && newEntry == null)
    {
      return 0;
    }

    // Get the patch between the two versions
    var patch = repo.Diff.Compare<Patch>(oldTree, newTree, new[] { normalizedPath });

    return CountChangedLinesInPatch(patch);
  }

  /// <summary>
  /// Count the number of changed lines in a file in staged changes (index vs HEAD).
  /// </summary>
  /// <param name="filePath">The relative path to the file</param>
  /// <returns>The number of changed lines in staged changes</returns>
  public int CountStagedChangedLines(string filePath)
  {
    using var repo = new Repository(_repositoryPath);

    // Normalize the file path
    var normalizedPath = filePath.Replace('\\', '/');

    // Compare index (staged) to HEAD
    var patch = repo.Diff.Compare<Patch>(repo.Head.Tip?.Tree, DiffTargets.Index, new[] { normalizedPath });

    return CountChangedLinesInPatch(patch);
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
    using var repo = new Repository(_repositoryPath);

    if (string.IsNullOrEmpty(oldCommitSha) || string.IsNullOrEmpty(newCommitSha))
    {
      return 0;
    }

    // Look up the commits
    var oldCommit = repo.Lookup<Commit>(oldCommitSha);
    var newCommit = repo.Lookup<Commit>(newCommitSha);

    if (oldCommit == null || newCommit == null)
    {
      return 0;
    }

    // Normalize the file path to use forward slashes
    var normalizedPath = filePath.Replace('\\', '/');

    // Get the tree entries for the file in both commits
    var oldTree = oldCommit.Tree;
    var newTree = newCommit.Tree;

    var oldEntry = oldTree?[normalizedPath];
    var newEntry = newTree?[normalizedPath];

    // If file doesn't exist in either commit, no changes
    if (oldEntry == null && newEntry == null)
    {
      return 0;
    }

    // Get the patch between the two versions
    var patch = repo.Diff.Compare<Patch>(oldTree, newTree, new[] { normalizedPath });

    return CountModifiedLinesInPatch(patch);
  }

  /// <summary>
  /// Count the number of modified lines (not additions) in staged changes (index vs HEAD).
  /// This counts line modifications (deletions + corresponding additions) but excludes pure additions.
  /// </summary>
  /// <param name="filePath">The relative path to the file</param>
  /// <returns>The number of modified lines in staged changes</returns>
  public int CountStagedModifiedLines(string filePath)
  {
    using var repo = new Repository(_repositoryPath);

    // Normalize the file path
    var normalizedPath = filePath.Replace('\\', '/');

    // Compare index (staged) to HEAD
    var patch = repo.Diff.Compare<Patch>(repo.Head.Tip?.Tree, DiffTargets.Index, new[] { normalizedPath });

    return CountModifiedLinesInPatch(patch);
  }

  /// <summary>
  /// Stage a specific file to the git index.
  /// </summary>
  /// <param name="filePath">The relative path to the file to stage</param>
  public void StageFile(string filePath)
  {
    using var repo = new Repository(_repositoryPath);
    LibGit2Sharp.Commands.Stage(repo, filePath);
  }

  /// <summary>
  /// Reset a specific file to match HEAD, removing changes from both index and working directory.
  /// This only affects the specified file and does not touch other files.
  /// </summary>
  /// <param name="filePath">The relative path to the file to reset</param>
  public void ResetFile(string filePath)
  {
    using var repo = new Repository(_repositoryPath);

    // Normalize the file path
    var normalizedPath = filePath.Replace('\\', '/');

    // Check if file exists in HEAD
    var headCommit = repo.Head.Tip;
    if (headCommit != null)
    {
      var headTree = headCommit.Tree;
      var fileExistsInHead = headTree[normalizedPath] != null;

      if (fileExistsInHead)
      {
        // Reset the file to HEAD (both index and working directory)
        repo.CheckoutPaths("HEAD", new[] { normalizedPath }, new CheckoutOptions
        {
          CheckoutModifiers = CheckoutModifiers.Force,
        });
      }
      else
      {
        // File doesn't exist in HEAD, just unstage and delete it
        try
        {
          LibGit2Sharp.Commands.Unstage(repo, normalizedPath);
        }
        catch
        {
          // Ignore errors if file wasn't staged
        }

        var absolutePath = Path.Combine(_repositoryPath, normalizedPath);
        if (File.Exists(absolutePath))
        {
          File.Delete(absolutePath);
        }
      }
    }
  }

  /// <summary>
  /// Count changed lines in a patch by looking for lines starting with + or -.
  /// </summary>
  private static int CountChangedLinesInPatch(Patch patch)
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

    return changedLines;
  }

  private static int CountModifiedLinesInPatch(Patch patch)
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

    // Each deletion has a corresponding addition (the modified line)
    return deletions * 2;
  }
}
