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
    Console.WriteLine("  init              Install FlowPilot into the current repository");
    Console.WriteLine("  new <plan-name>   Create a new FlowPilot plan");
    Console.WriteLine("  next <plan-name>  Advance to the next phase of the plan");
    Console.WriteLine("  lint <plan-name>  Validate the plan follows FlowPilot rules");
    Console.WriteLine("  help              Display this help information");
    Console.WriteLine();
    Console.WriteLine("Getting Started:");
    Console.WriteLine("  1. flowpilot init                 # Install FlowPilot");
    Console.WriteLine("  2. flowpilot new my-feature       # Create a new plan");
    Console.WriteLine("  3. flowpilot next my-feature      # Advance through phases");
    Console.WriteLine("  4. flowpilot lint my-feature      # Validate your plan");

    return Task.FromResult(0);
  }
}
