using FlowPilot.Cli.Models;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Interface for discrete linting rules.
/// </summary>
public interface ILintingRule
{
  /// <summary>
  /// Execute the linting rule and add any errors found to the context.
  /// </summary>
  Task ExecuteAsync(PlanContext context);
}
