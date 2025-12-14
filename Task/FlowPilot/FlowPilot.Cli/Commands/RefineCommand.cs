using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the refine command.
/// </summary>
public class RefineCommand : ICommand
{
  private readonly ILogger<RefineCommand> _logger;

  public RefineCommand(ILogger<RefineCommand> logger)
  {
    _logger = logger;
  }

  public string Name => "refine";

  public string Description => "Analyze unsuccessful implementation attempts and present options to the user for guidance.";

  public Task<int> ExecuteAsync(string[] args)
  {
    _logger.LogInformation("It looks like implementation is not going according to plan and some challenges have arisen.");
    _logger.LogInformation("Based on your understanding of the challenges, present a series of options in the .flowpilot/template/key-decisions.md format and report them to the user for guidance.");

    return Task.FromResult(0);
  }
}
