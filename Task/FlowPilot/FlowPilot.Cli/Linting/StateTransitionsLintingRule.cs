using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that state transitions are checked in order.
/// </summary>
public class StateTransitionsLintingRule : ILintingRule
{
  private readonly IFileSystemService _fileSystem;
  private readonly StateParser _stateParser;

  public StateTransitionsLintingRule(IFileSystemService fileSystem, StateParser stateParser)
  {
    _fileSystem = fileSystem;
    _stateParser = stateParser;
  }

  public Task ExecuteAsync(PlanContext context)
  {
    var content = _fileSystem.ReadAllText(context.StateFilePath);
    var items = _stateParser.ParseStateFile(content);

    // Check that items are checked in order
    bool foundUnchecked = false;

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
        context.LintingErrors.Add($"State [{item.Identifier}] is checked but previous items are not checked. Items must be checked in order.");
      }
    }

    // Check hard boundaries
    if (context.State.HasKeyDecisions && context.State.HasPhaseAnalysis)
    {
      context.LintingErrors.Add("Cannot check [phase-analysis] in the same branch as [key-decisions]. A new branch is required.");
    }

    if (context.State.HasPhaseAnalysis && context.State.HasPhaseDetails)
    {
      context.LintingErrors.Add("Cannot check [phase-n-details] in the same branch as [phase-analysis]. A new branch is required.");
    }

    return Task.CompletedTask;
  }
}
