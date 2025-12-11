namespace FlowPilot.Cli.Commands;

/// <summary>
/// Interface for FlowPilot commands.
/// </summary>
public interface ICommand
{
  /// <summary>
  /// Gets the name of the command.
  /// </summary>
  string Name { get; }

  /// <summary>
  /// Gets the description of the command.
  /// </summary>
  string Description { get; }

  /// <summary>
  /// Execute the command.
  /// </summary>
  Task<int> ExecuteAsync(string[] args);
}
