using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that phase-n-details.md files have been modified from the template.
/// </summary>
public class PhaseDetailsTemplateChangedRule : ILintingRule
{
  private const string TemplateReplaceMe = "Replace me";
  private const string TemplateUpdateMe = "update me";

  private readonly IFileSystemService _fileSystem;
  private readonly TemplateService _templateService;

  public PhaseDetailsTemplateChangedRule(IFileSystemService fileSystem, TemplateService templateService)
  {
    _fileSystem = fileSystem;
    _templateService = templateService;
  }

  public Task ExecuteAsync(PlanContext context)
  {
    if (!context.State.HasPhaseDetails)
    {
      return Task.CompletedTask;
    }

    var template = _templateService.ReadTemplate("phase-n-details.md");

    foreach (var phase in context.State.Phases)
    {
      var phaseFile = Path.Combine(context.PlanSubDirectory, $"phase-{phase.PhaseNumber}-details.md");

      if (!_fileSystem.FileExists(phaseFile))
      {
        context.LintingErrors.Add($"phase-{phase.PhaseNumber}-details.md does not exist");
      }
      else
      {
        var content = _fileSystem.ReadAllText(phaseFile);

        if (IsTemplateUnchanged(content, template))
        {
          context.LintingErrors.Add($"phase-{phase.PhaseNumber}-details.md has not been modified from the template");
        }
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
