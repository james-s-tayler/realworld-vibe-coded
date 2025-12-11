using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that phase-analysis.md has been modified from the template.
/// </summary>
public class PhaseAnalysisTemplateChangedRule : ILintingRule
{
  private const string TemplateReplaceMe = "Replace me";
  private const string TemplateUpdateMe = "update me";

  private readonly IFileSystemService _fileSystem;
  private readonly TemplateService _templateService;

  public PhaseAnalysisTemplateChangedRule(IFileSystemService fileSystem, TemplateService templateService)
  {
    _fileSystem = fileSystem;
    _templateService = templateService;
  }

  public Task ExecuteAsync(PlanContext context)
  {
    if (!context.State.HasPhaseAnalysis)
    {
      return Task.CompletedTask;
    }

    var phaseAnalysisPath = Path.Combine(context.MetaDirectory, "phase-analysis.md");

    if (!_fileSystem.FileExists(phaseAnalysisPath))
    {
      context.LintingErrors.Add("phase-analysis.md does not exist but [phase-analysis] is checked in state.md");
    }
    else
    {
      var content = _fileSystem.ReadAllText(phaseAnalysisPath);
      var template = _templateService.ReadTemplate("phase-analysis.md");

      if (IsTemplateUnchanged(content, template))
      {
        context.LintingErrors.Add("phase-analysis.md has not been modified from the template");
      }
    }

    return Task.CompletedTask;
  }

  private bool IsTemplateUnchanged(string content, string template)
  {
    var normalizedContent = content.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    var normalizedTemplate = template.Replace("\r\n", "\n").Replace("\r", "\n").Trim();

    return normalizedContent == normalizedTemplate || normalizedContent.Contains(TemplateReplaceMe) || normalizedContent.Contains(TemplateUpdateMe);
  }
}
