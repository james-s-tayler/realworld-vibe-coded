using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the help command.
/// </summary>
public class HelpCommand : ICommand
{
  private readonly ILogger<HelpCommand> _logger;

  public HelpCommand(ILogger<HelpCommand> logger)
  {
    _logger = logger;
  }

  public string Name => "help";

  public string Description => "Display help information";

  public Task<int> ExecuteAsync(string[] args)
  {
    _logger.LogInformation("FlowPilot CLI - Orchestration tool for multi-stage feature development");
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Usage: flowpilot <command> [options]");
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Commands:");
    _logger.LogInformation("  init              Install FlowPilot into the current repository");
    _logger.LogInformation("  new <plan-name>   Create a new FlowPilot plan");
    _logger.LogInformation("  next <plan-name>  Advance to the next phase of the plan");
    _logger.LogInformation("  lint <plan-name>  Validate the plan follows FlowPilot rules");
    _logger.LogInformation("  verify <plan-name> Check verification requirements for the current phase");
    _logger.LogInformation("  stuck             Analyze unsuccessful implementation attempts and present options to the user for guidance.");
    _logger.LogInformation("  help              Display this help information");
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Getting Started:");
    _logger.LogInformation("  1. flowpilot init                 # Install FlowPilot");
    _logger.LogInformation("  2. flowpilot new my-feature       # Create a new plan");
    _logger.LogInformation("  3. flowpilot next my-feature      # Advance through phases");
    _logger.LogInformation("  4. flowpilot lint my-feature      # Validate your plan");
    _logger.LogInformation("  5. flowpilot verify my-feature    # Check phase verification requirements");

    return Task.FromResult(0);
  }
}
