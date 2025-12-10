using System.CommandLine;

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

    // Implementation will be added in Phase 2
    await Task.CompletedTask;
  }
}
