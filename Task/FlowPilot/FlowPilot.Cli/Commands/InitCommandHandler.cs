using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the init command.
/// </summary>
public class InitCommandHandler : ICommand
{
  private readonly IFileSystemService _fileSystem;
  private readonly TemplateService _templateService;
  private readonly StateParser _stateParser;
  private readonly PlanManager _planManager;

  public InitCommandHandler(
    IFileSystemService fileSystem,
    TemplateService templateService,
    StateParser stateParser,
    PlanManager planManager)
  {
    _fileSystem = fileSystem;
    _templateService = templateService;
    _stateParser = stateParser;
    _planManager = planManager;
  }

  public string Name => "init";

  public string Description => "Initialize a new FlowPilot plan";

  public Task<int> ExecuteAsync(string[] args)
  {
    if (args.Length == 0)
    {
      Console.WriteLine("Error: Plan name is required.");
      Console.WriteLine("Usage: flowpilot init <plan-name>");
      return Task.FromResult(1);
    }

    var planName = args[0];
    Console.WriteLine($"Initializing FlowPilot plan: {planName}");

    if (_planManager.PlanExists(planName))
    {
      Console.WriteLine($"Error: Plan '{planName}' already exists.");
      return Task.FromResult(1);
    }

    _planManager.InitializePlan(planName);

    Console.WriteLine($"✓ Plan '{planName}' initialized successfully.");
    Console.WriteLine();
    Console.WriteLine("Next steps:");
    Console.WriteLine($"1. Update .flowpilot/plans/{planName}/meta/goal.md with your feature requirements");
    Console.WriteLine($"2. Commit this change to your repository");
    Console.WriteLine($"3. Run 'flowpilot next {planName}' to continue");

    return Task.FromResult(0);
  }
}
