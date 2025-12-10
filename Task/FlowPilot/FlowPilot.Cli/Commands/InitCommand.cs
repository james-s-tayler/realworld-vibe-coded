using System.CommandLine;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Commands;

public static class InitCommand
{
  public static Command Create()
  {
    var planNameArgument = new Argument<string>(
      name: "plan-name",
      description: "Name of the plan to initialize");

    var command = new Command("init", "Initialize a new FlowPilot plan")
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
    Console.WriteLine($"Initializing FlowPilot plan: {planName}");

    var fileSystem = new FileSystemService();
    var templateService = new TemplateService();
    var stateParser = new StateParser();
    var planManager = new PlanManager(fileSystem, templateService, stateParser);

    if (planManager.PlanExists(planName))
    {
      Console.WriteLine($"Error: Plan '{planName}' already exists.");
      Environment.Exit(1);
      return;
    }

    planManager.InitializePlan(planName);

    Console.WriteLine($"✓ Plan '{planName}' initialized successfully.");
    Console.WriteLine();
    Console.WriteLine("Next steps:");
    Console.WriteLine($"1. Update .flowpilot/plans/{planName}/meta/goal.md with your feature requirements");
    Console.WriteLine($"2. Commit this change to your repository");
    Console.WriteLine($"3. Run 'flowpilot next {planName}' to continue");

    await Task.CompletedTask;
  }
}
