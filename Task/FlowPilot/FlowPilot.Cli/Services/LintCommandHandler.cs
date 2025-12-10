using FlowPilot.Cli.Models;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Handles the logic for the 'lint' command.
/// </summary>
public class LintCommandHandler
{
  private readonly PlanManager _planManager;
  private readonly IFileSystemService _fileSystem;
  private readonly GitService _gitService;
  private readonly TemplateService _templateService;

  public LintCommandHandler(
    PlanManager planManager,
    IFileSystemService fileSystem,
    GitService gitService,
    TemplateService templateService)
  {
    _planManager = planManager;
    _fileSystem = fileSystem;
    _gitService = gitService;
    _templateService = templateService;
  }

  public int Execute(string planName)
  {
    var errors = new List<string>();

    // Get current state
    var state = _planManager.GetCurrentState(planName);

    if (!state.IsInitialized)
    {
      errors.Add("Plan not initialized");
      PrintErrors(errors);
      return 1;
    }

    // Check for state.md changes
    var stateChanges = CheckStateChanges(planName);

    if (stateChanges > 1)
    {
      errors.Add($"Multiple state.md changes detected ({stateChanges}). Only one state transition is allowed per commit.");
    }

    // Validate transitions
    var transitionErrors = ValidateStateTransitions(planName, state);
    errors.AddRange(transitionErrors);

    // Check for template changes based on current state
    var templateErrors = CheckTemplateChanges(planName, state);
    errors.AddRange(templateErrors);

    // Check URL validity in references.md if it exists and is checked
    if (state.HasReferences)
    {
      var urlErrors = CheckReferencesUrls(planName);
      errors.AddRange(urlErrors);
    }

    if (errors.Count > 0)
    {
      PrintErrors(errors);
      return 1;
    }

    Console.WriteLine("✓ Lint passed - plan follows FlowPilot rules");
    return 0;
  }

  private int CheckStateChanges(string planName)
  {
    var stateFilePath = _planManager.GetStateFilePath(planName);
    var changedFiles = _gitService.GetChangedFiles();
    var repoRoot = _gitService.GetRepositoryRoot();

    // Normalize paths
    var relativeStatePath = Path.GetRelativePath(repoRoot, stateFilePath);

    // Count how many times state.md appears in changed files (using cross-platform path comparison)
    var normalizedStatePath = relativeStatePath.Replace('\\', '/');
    return changedFiles.Count(f => f.Replace('\\', '/').Equals(normalizedStatePath, StringComparison.OrdinalIgnoreCase));
  }

  private List<string> ValidateStateTransitions(string planName, PlanState state)
  {
    var errors = new List<string>();
    var stateFilePath = _planManager.GetStateFilePath(planName);
    var content = _fileSystem.ReadAllText(stateFilePath);
    var parser = new StateParser();
    var items = parser.ParseStateFile(content);

    // Check that items are checked in order
    bool foundUnchecked = false;
    var previousItem = string.Empty;

    foreach (var item in items)
    {
      // Skip phase items for this check
      if (item.PhaseNumber.HasValue)
      {
        continue;
      }

      if (!item.IsChecked)
      {
        foundUnchecked = true;
      }
      else if (foundUnchecked)
      {
        errors.Add($"State [{item.Identifier}] is checked but previous items are not checked. Items must be checked in order.");
      }

      previousItem = item.Identifier;
    }

    // Check hard boundaries
    if (state.HasKeyDecisions && state.HasPhaseAnalysis)
    {
      errors.Add("Cannot check [phase-analysis] in the same branch as [key-decisions]. A new branch is required.");
    }

    if (state.HasPhaseAnalysis && state.HasPhaseDetails)
    {
      errors.Add("Cannot check [phase-n-details] in the same branch as [phase-analysis]. A new branch is required.");
    }

    return errors;
  }

  private List<string> CheckTemplateChanges(string planName, PlanState state)
  {
    var errors = new List<string>();
    var metaDir = _planManager.GetMetaDirectory(planName);

    // Check goal.md - should be changed from template if state is initialized
    if (state.IsInitialized)
    {
      var goalPath = Path.Combine(metaDir, "goal.md");

      if (_fileSystem.FileExists(goalPath))
      {
        var content = _fileSystem.ReadAllText(goalPath);
        var template = _templateService.ReadTemplate("goal.md");

        if (content.Trim() == template.Trim())
        {
          errors.Add("goal.md has not been modified from the template. Update it with your feature requirements.");
        }
      }
    }

    // Check references.md - should be changed if references is checked
    if (state.HasReferences)
    {
      var referencesPath = Path.Combine(metaDir, "references.md");

      if (!_fileSystem.FileExists(referencesPath))
      {
        errors.Add("references.md does not exist but [references] is checked in state.md");
      }
      else
      {
        var content = _fileSystem.ReadAllText(referencesPath);
        var template = _templateService.ReadTemplate("references.md");

        if (IsTemplateUnchanged(content, template))
        {
          errors.Add("references.md has not been modified from the template");
        }
      }
    }

    // Check system-analysis.md
    if (state.HasSystemAnalysis)
    {
      var analysisPath = Path.Combine(metaDir, "system-analysis.md");

      if (!_fileSystem.FileExists(analysisPath))
      {
        errors.Add("system-analysis.md does not exist but [system-analysis] is checked in state.md");
      }
      else
      {
        var content = _fileSystem.ReadAllText(analysisPath);
        var template = _templateService.ReadTemplate("system-analysis.md");

        if (IsTemplateUnchanged(content, template))
        {
          errors.Add("system-analysis.md has not been modified from the template");
        }
      }
    }

    // Check key-decisions.md
    if (state.HasKeyDecisions)
    {
      var decisionsPath = Path.Combine(metaDir, "key-decisions.md");

      if (!_fileSystem.FileExists(decisionsPath))
      {
        errors.Add("key-decisions.md does not exist but [key-decisions] is checked in state.md");
      }
      else
      {
        var content = _fileSystem.ReadAllText(decisionsPath);
        var template = _templateService.ReadTemplate("key-decisions.md");

        if (IsTemplateUnchanged(content, template))
        {
          errors.Add("key-decisions.md has not been modified from the template");
        }
      }
    }

    // Check phase-analysis.md
    if (state.HasPhaseAnalysis)
    {
      var phaseAnalysisPath = Path.Combine(metaDir, "phase-analysis.md");

      if (!_fileSystem.FileExists(phaseAnalysisPath))
      {
        errors.Add("phase-analysis.md does not exist but [phase-analysis] is checked in state.md");
      }
      else
      {
        var content = _fileSystem.ReadAllText(phaseAnalysisPath);
        var template = _templateService.ReadTemplate("phase-analysis.md");

        if (IsTemplateUnchanged(content, template))
        {
          errors.Add("phase-analysis.md has not been modified from the template");
        }
      }
    }

    // Check phase-n-details files
    if (state.HasPhaseDetails)
    {
      var planDir = _planManager.GetPlanSubDirectory(planName);
      var template = _templateService.ReadTemplate("phase-n-details.md");

      foreach (var phase in state.Phases)
      {
        var phaseFile = Path.Combine(planDir, $"phase-{phase.PhaseNumber}-details.md");

        if (!_fileSystem.FileExists(phaseFile))
        {
          errors.Add($"phase-{phase.PhaseNumber}-details.md does not exist");
        }
        else
        {
          var content = _fileSystem.ReadAllText(phaseFile);

          if (IsTemplateUnchanged(content, template))
          {
            errors.Add($"phase-{phase.PhaseNumber}-details.md has not been modified from the template");
          }
        }
      }
    }

    return errors;
  }

  private List<string> CheckReferencesUrls(string planName)
  {
    var errors = new List<string>();
    var metaDir = _planManager.GetMetaDirectory(planName);
    var referencesPath = Path.Combine(metaDir, "references.md");

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
        var response = httpClient.Send(request);

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

  private bool IsTemplateUnchanged(string content, string template)
  {
    // Remove whitespace variations for comparison
    var normalizedContent = content.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    var normalizedTemplate = template.Replace("\r\n", "\n").Replace("\r", "\n").Trim();

    // Check if content contains only template text (allowing for minor formatting differences)
    return normalizedContent == normalizedTemplate || normalizedContent.Contains("Replace me") || normalizedContent.Contains("update me");
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
