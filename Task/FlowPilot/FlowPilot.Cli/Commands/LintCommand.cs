using System.CommandLine;

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
        await ExecuteAsync(planName);
      },
      planNameArgument);

    return command;
  }

  private static async Task ExecuteAsync(string planName)
  {
    Console.WriteLine($"Linting FlowPilot plan: {planName}");

    // Implementation will be added in Phase 3
    await Task.CompletedTask;
  }
}
