using System.CommandLine;
using FlowPilot.Cli.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FlowPilot.Cli.Commands;

public static class LintCommand
{
  public static Command Create()
  {
    var planNameArgument = new Argument<string>(
      name: "plan-name",
      description: "Name of the plan to lint");

    var command = new Command("lint", "Validate the plan follows FlowPilot rules")
    {
      planNameArgument,
    };

    command.SetHandler(
      async (planName) =>
      {
        var exitCode = await ExecuteAsync(planName);
        Environment.Exit(exitCode);
      },
      planNameArgument);

    return command;
  }

  private static async Task<int> ExecuteAsync(string planName)
  {
    // Create service provider
    var fileSystem = new FileSystemService();
    var currentDir = fileSystem.GetCurrentDirectory();

    var services = new ServiceCollection();
    services.ConfigureServices(currentDir);
    var serviceProvider = services.BuildServiceProvider();

    var planManager = serviceProvider.GetRequiredService<PlanManager>();
    if (!planManager.PlanExists(planName))
    {
      Console.WriteLine($"Error: Plan '{planName}' not found.");
      return 1;
    }

    var gitService = serviceProvider.GetRequiredService<GitService>();
    try
    {
      gitService.GetRepositoryRoot();
    }
    catch (InvalidOperationException)
    {
      Console.WriteLine("Error: Not in a git repository. FlowPilot lint requires git.");
      return 1;
    }

    var handler = serviceProvider.GetRequiredService<LintCommandHandler>();
    var exitCode = await handler.ExecuteAsync(planName);
    return exitCode;
  }
}
