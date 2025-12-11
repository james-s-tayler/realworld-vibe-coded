using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that key-decisions.md has been modified from the template.
/// </summary>
public class KeyDecisionsTemplateChangedRule : ILintingRule
{
  private const string TemplateReplaceMe = "Replace me";
  private const string TemplateUpdateMe = "update me";

  private readonly IFileSystemService _fileSystem;
  private readonly TemplateService _templateService;

  public KeyDecisionsTemplateChangedRule(IFileSystemService fileSystem, TemplateService templateService)
  {
    _fileSystem = fileSystem;
    _templateService = templateService;
  }

  public Task ExecuteAsync(PlanContext context)
  {
    if (!context.State.HasKeyDecisions)
    {
      return Task.CompletedTask;
    }

    var decisionsPath = Path.Combine(context.MetaDirectory, "key-decisions.md");

    if (!_fileSystem.FileExists(decisionsPath))
    {
      context.LintingErrors.Add("key-decisions.md does not exist but [key-decisions] is checked in state.md");
    }
    else
    {
      var content = _fileSystem.ReadAllText(decisionsPath);
      var template = _templateService.ReadTemplate("key-decisions.md");

      if (IsTemplateUnchanged(content, template))
      {
        context.LintingErrors.Add("key-decisions.md has not been modified from the template");
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
