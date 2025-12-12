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
}
