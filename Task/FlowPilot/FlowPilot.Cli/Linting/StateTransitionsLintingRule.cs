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

    // Hard boundaries are enforced by the workflow (documented in instructions),
    // not by lint. The tool trusts developers to follow the branch workflow.
    // Lint only validates that state transitions are in order and that
    // only one state change happens per commit (checked by StateChangesLintingRule).

    return Task.CompletedTask;
  }
}
