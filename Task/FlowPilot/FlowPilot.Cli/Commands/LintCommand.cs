using System.CommandLine;
using FlowPilot.Cli.Services;

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
    // Create services
    IFileSystemService fileSystem = new FileSystemService();
    var templateService = new TemplateService();
    var stateParser = new StateParser();
    var planManager = new PlanManager(fileSystem, templateService, stateParser);

    if (!planManager.PlanExists(planName))
    {
      Console.WriteLine($"Error: Plan '{planName}' not found.");
      return 1;
    }

    var currentDir = fileSystem.GetCurrentDirectory();
    var gitService = new GitService(currentDir);

    try
    {
      gitService.GetRepositoryRoot();
    }
    catch (InvalidOperationException)
    {
      Console.WriteLine("Error: Not in a git repository. FlowPilot lint requires git.");
      return 1;
    }

    // Create service factory and get linting rules
    var serviceFactory = new ServiceFactory(fileSystem, templateService, stateParser, gitService, planManager);
    var lintingRules = serviceFactory.CreateLintingRules();

    var handler = new LintCommandHandler(planManager, lintingRules);
    var exitCode = await handler.ExecuteAsync(planName);
    return exitCode;
  }
}
