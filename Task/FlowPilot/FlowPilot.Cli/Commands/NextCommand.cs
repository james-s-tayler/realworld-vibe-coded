using System.CommandLine;

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
    Console.WriteLine($"Advancing FlowPilot plan: {planName}");

    // Implementation will be added in Phase 2
    await Task.CompletedTask;
  }
}
