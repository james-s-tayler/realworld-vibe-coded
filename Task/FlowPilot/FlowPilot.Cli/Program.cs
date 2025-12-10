using System.CommandLine;
using FlowPilot.Cli.Commands;

namespace FlowPilot.Cli;

public class Program
{
  public static async Task<int> Main(string[] args)
  {
    var rootCommand = new RootCommand("FlowPilot CLI - Orchestration tool for multi-stage feature development");

    // Add commands
    rootCommand.AddCommand(InitCommand.Create());
    rootCommand.AddCommand(NextCommand.Create());
    rootCommand.AddCommand(LintCommand.Create());

    return await rootCommand.InvokeAsync(args);
  }
}
