using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that template files have been modified from their defaults.
/// </summary>
public class TemplateChangesLintingRule : ILintingRule
{
  private const string TemplateReplaceMe = "Replace me";
  private const string TemplateUpdateMe = "update me";

  private readonly IFileSystemService _fileSystem;
  private readonly TemplateService _templateService;

  public TemplateChangesLintingRule(IFileSystemService fileSystem, TemplateService templateService)
  {
    _fileSystem = fileSystem;
    _templateService = templateService;
  }

  public Task<List<string>> ExecuteAsync(PlanContext context)
  {
    var errors = new List<string>();

    // Check goal.md - should be changed from template if state is initialized
    if (context.State.IsInitialized)
    {
      var goalPath = Path.Combine(context.MetaDirectory, "goal.md");

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
    if (context.State.HasReferences)
    {
      var referencesPath = Path.Combine(context.MetaDirectory, "references.md");

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
    if (context.State.HasSystemAnalysis)
    {
      var analysisPath = Path.Combine(context.MetaDirectory, "system-analysis.md");

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
    if (context.State.HasKeyDecisions)
    {
      var decisionsPath = Path.Combine(context.MetaDirectory, "key-decisions.md");

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
    if (context.State.HasPhaseAnalysis)
    {
      var phaseAnalysisPath = Path.Combine(context.MetaDirectory, "phase-analysis.md");

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
    if (context.State.HasPhaseDetails)
    {
      var template = _templateService.ReadTemplate("phase-n-details.md");

      foreach (var phase in context.State.Phases)
      {
        var phaseFile = Path.Combine(context.PlanSubDirectory, $"phase-{phase.PhaseNumber}-details.md");

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

    return Task.FromResult(errors);
  }

  private bool IsTemplateUnchanged(string content, string template)
  {
    // Remove whitespace variations for comparison
    var normalizedContent = content.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    var normalizedTemplate = template.Replace("\r\n", "\n").Replace("\r", "\n").Trim();

    // Check if content contains only template text (allowing for minor formatting differences)
    return normalizedContent == normalizedTemplate || normalizedContent.Contains(TemplateReplaceMe) || normalizedContent.Contains(TemplateUpdateMe);
  }
}
