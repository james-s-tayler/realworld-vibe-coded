using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that goal.md has been modified from the template.
/// </summary>
public class GoalTemplateChangedRule : ILintingRule
{
  private readonly IFileSystemService _fileSystem;
  private readonly TemplateService _templateService;

  public GoalTemplateChangedRule(IFileSystemService fileSystem, TemplateService templateService)
  {
    _fileSystem = fileSystem;
    _templateService = templateService;
  }

  public Task ExecuteAsync(PlanContext context)
  {
    if (!context.State.IsInitialized)
    {
      return Task.CompletedTask;
    }

    var goalPath = Path.Combine(context.MetaDirectory, "goal.md");

    if (_fileSystem.FileExists(goalPath))
    {
      var content = _fileSystem.ReadAllText(goalPath);
      var template = _templateService.ReadTemplate("goal.md");

      if (content.Trim() == template.Trim())
      {
        context.LintingErrors.Add("goal.md has not been modified from the template. Update it with your feature requirements.");
      }
    }

    return Task.CompletedTask;
  }
}
