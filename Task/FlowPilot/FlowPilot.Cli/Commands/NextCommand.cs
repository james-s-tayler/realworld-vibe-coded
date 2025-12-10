using System.CommandLine;
using FlowPilot.Cli.Services;

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
    // Create services
    IFileSystemService fileSystem = new FileSystemService();
    var templateService = new TemplateService();
    var stateParser = new StateParser();
    var planManager = new PlanManager(fileSystem, templateService, stateParser);

    if (!planManager.PlanExists(planName))
    {
      Console.WriteLine($"Error: Plan '{planName}' not found. Run 'flowpilot init {planName}' first.");
      Environment.Exit(1);
      return;
    }

    // Run lint first
    var currentDir = fileSystem.GetCurrentDirectory();
    var gitService = new GitService(currentDir);

    try
    {
      gitService.GetRepositoryRoot();

      // Create service factory and get linting rules
      var serviceFactory = new ServiceFactory(fileSystem, templateService, stateParser, gitService, planManager);
      var lintingRules = serviceFactory.CreateLintingRules();

      var lintHandler = new LintCommandHandler(planManager, lintingRules);
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

    // Create state transitions and execute next
    var serviceFactory2 = new ServiceFactory(fileSystem, templateService, stateParser, gitService, planManager);
    var stateTransitions = serviceFactory2.CreateStateTransitions();

    var handler = new NextCommandHandler(planManager, stateTransitions);
    handler.Execute(planName);

    await Task.CompletedTask;
  }
}
