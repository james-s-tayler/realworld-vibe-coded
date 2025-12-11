using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that system-analysis.md has been modified from the template.
/// </summary>
public class SystemAnalysisTemplateChangedRule : ILintingRule
{
  private const string TemplateReplaceMe = "Replace me";
  private const string TemplateUpdateMe = "update me";

  private readonly IFileSystemService _fileSystem;
  private readonly TemplateService _templateService;

  public SystemAnalysisTemplateChangedRule(IFileSystemService fileSystem, TemplateService templateService)
  {
    _fileSystem = fileSystem;
    _templateService = templateService;
  }

  public Task ExecuteAsync(PlanContext context)
  {
    if (!context.State.HasSystemAnalysis)
    {
      return Task.CompletedTask;
    }

    var analysisPath = Path.Combine(context.MetaDirectory, "system-analysis.md");

    if (!_fileSystem.FileExists(analysisPath))
    {
      context.LintingErrors.Add("system-analysis.md does not exist but [system-analysis] is checked in state.md");
    }
    else
    {
      var content = _fileSystem.ReadAllText(analysisPath);
      var template = _templateService.ReadTemplate("system-analysis.md");

      if (IsTemplateUnchanged(content, template))
      {
        context.LintingErrors.Add("system-analysis.md has not been modified from the template");
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
