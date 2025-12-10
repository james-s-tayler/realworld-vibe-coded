using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that URLs in references.md are accessible.
/// </summary>
public class ReferencesUrlLintingRule : ILintingRule
{
  private readonly IFileSystemService _fileSystem;

  public ReferencesUrlLintingRule(IFileSystemService fileSystem)
  {
    _fileSystem = fileSystem;
  }

  public async Task<List<string>> ExecuteAsync(PlanContext context)
  {
    var errors = new List<string>();

    if (!context.State.HasReferences)
    {
      return errors;
    }

    var referencesPath = Path.Combine(context.MetaDirectory, "references.md");

    if (!_fileSystem.FileExists(referencesPath))
    {
      return errors;
    }

    var content = _fileSystem.ReadAllText(referencesPath);

    // Extract URLs from markdown links [text](URL)
    var urlPattern = @"\[.+?\]\((https?://[^\)]+)\)";
    var matches = System.Text.RegularExpressions.Regex.Matches(content, urlPattern);

    using var httpClient = new HttpClient();
    httpClient.Timeout = TimeSpan.FromSeconds(10);

    foreach (System.Text.RegularExpressions.Match match in matches)
    {
      var url = match.Groups[1].Value;

      try
      {
        var request = new HttpRequestMessage(HttpMethod.Head, url);
        var response = await httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
          errors.Add($"URL returns 404 Not Found: {url}");
        }
      }
      catch (Exception ex)
      {
        errors.Add($"Failed to validate URL {url}: {ex.Message}");
      }
    }

    return errors;
  }
}
