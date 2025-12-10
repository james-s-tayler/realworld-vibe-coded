using FlowPilot.Cli.Models;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Interface for discrete linting rules.
/// </summary>
public interface ILintingRule
{
  /// <summary>
  /// Execute the linting rule and return any errors found.
  /// </summary>
  Task<List<string>> ExecuteAsync(PlanContext context);
}
