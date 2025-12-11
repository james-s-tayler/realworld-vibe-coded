using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that references.md has been modified from the template.
/// </summary>
public class ReferencesTemplateChangedRule : ILintingRule
{
  private const string TemplateReplaceMe = "Replace me";
  private const string TemplateUpdateMe = "update me";

  private readonly IFileSystemService _fileSystem;
  private readonly TemplateService _templateService;

  public ReferencesTemplateChangedRule(IFileSystemService fileSystem, TemplateService templateService)
  {
    _fileSystem = fileSystem;
    _templateService = templateService;
  }

  public Task ExecuteAsync(PlanContext context)
  {
    if (!context.State.HasReferences)
    {
      return Task.CompletedTask;
    }

    var referencesPath = Path.Combine(context.MetaDirectory, "references.md");

    if (!_fileSystem.FileExists(referencesPath))
    {
      context.LintingErrors.Add("references.md does not exist but [references] is checked in state.md");
    }
    else
    {
      var content = _fileSystem.ReadAllText(referencesPath);
      var template = _templateService.ReadTemplate("references.md");

      if (IsTemplateUnchanged(content, template))
      {
        context.LintingErrors.Add("references.md has not been modified from the template");
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
