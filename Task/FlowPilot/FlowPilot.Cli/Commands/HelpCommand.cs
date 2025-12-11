namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the help command.
/// </summary>
public class HelpCommand : ICommand
{
  public string Name => "help";

  public string Description => "Display help information";

  public Task<int> ExecuteAsync(string[] args)
  {
    Console.WriteLine("FlowPilot CLI - Orchestration tool for multi-stage feature development");
    Console.WriteLine();
    Console.WriteLine("Usage: flowpilot <command> [options]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  init <plan-name>  Initialize a new FlowPilot plan");
    Console.WriteLine("  next <plan-name>  Advance to the next phase of the plan");
    Console.WriteLine("  lint <plan-name>  Validate the plan follows FlowPilot rules");
    Console.WriteLine("  help              Display this help information");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  flowpilot init my-feature");
    Console.WriteLine("  flowpilot next my-feature");
    Console.WriteLine("  flowpilot lint my-feature");

    return Task.FromResult(0);
  }
}
