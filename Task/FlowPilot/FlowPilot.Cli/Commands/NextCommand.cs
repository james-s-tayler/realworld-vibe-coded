using System.CommandLine;
using FlowPilot.Cli.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FlowPilot.Cli.Commands;

public static class NextCommand
{
  public static Command Create()
  {
    var planNameArgument = new Argument<string>(
      name: "plan-name",
      description: "Name of the plan to advance");

    var command = new Command("next", "Advance to the next phase of the plan")
    {
      planNameArgument,
    };

    command.SetHandler(
      async (planName) =>
      {
        await ExecuteAsync(planName);
      },
      planNameArgument);

    return command;
  }

  private static async Task ExecuteAsync(string planName)
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
      Console.WriteLine($"Error: Plan '{planName}' not found. Run 'flowpilot init {planName}' first.");
      Environment.Exit(1);
      return;
    }

    // Run lint first
    var gitService = serviceProvider.GetRequiredService<GitService>();
    try
    {
      gitService.GetRepositoryRoot();

      var lintHandler = serviceProvider.GetRequiredService<LintCommandHandler>();
      var lintResult = await lintHandler.ExecuteAsync(planName);

      if (lintResult != 0)
      {
        Console.WriteLine();
        Console.WriteLine("❌ Cannot proceed - lint check failed. Fix the issues above first.");
        Environment.Exit(1);
        return;
      }
    }
    catch (InvalidOperationException)
    {
      // Not in git repo - continue without lint (for testing purposes)
      Console.WriteLine("⚠️  Warning: Not in a git repository. Skipping lint check.");
    }

    var handler = serviceProvider.GetRequiredService<NextCommandHandler>();
    handler.Execute(planName);

    await Task.CompletedTask;
  }
}
