using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the verify command.
/// </summary>
public class VerifyCommandExecutor : ICommand
{
  private readonly PlanManager _planManager;
  private readonly IFileSystemService _fileSystem;
  private readonly ILogger<VerifyCommandExecutor> _logger;

  public VerifyCommandExecutor(
    PlanManager planManager,
    IFileSystemService fileSystem,
    ILogger<VerifyCommandExecutor> logger)
  {
    _planManager = planManager;
    _fileSystem = fileSystem;
    _logger = logger;
  }

  public string Name => "verify";

  public string Description => "Check verification requirements for the current phase";

  public Task<int> ExecuteAsync(string[] args)
  {
    var (planName, shouldExit, exitCode) = _planManager.ResolvePlanName(
      args,
      msg => _logger.LogInformation(msg),
      msg => _logger.LogError(msg));

    if (shouldExit)
    {
      if (exitCode == 1)
      {
        _logger.LogInformation("Usage: flowpilot verify <plan-name>");
      }

      return Task.FromResult(exitCode);
    }

    // Get current state to find the active phase
    var state = _planManager.GetCurrentState(planName!);

    if (!state.IsInitialized)
    {
      _logger.LogError("Plan not initialized. Run 'flowpilot new' first");
      return Task.FromResult(1);
    }

    // Find the current phase (first incomplete phase or last phase if all complete)
    var currentPhase = state.Phases.FirstOrDefault(p => !p.IsComplete);

    // If all phases are complete, check the last phase
    if (currentPhase == null && state.Phases.Any())
    {
      currentPhase = state.Phases.Last();
      _logger.LogInformation("All phases are complete. Checking verification for the last phase.");
    }

    if (currentPhase == null)
    {
      // Check if we're still in planning stages
      if (!state.HasPhaseAnalysis)
      {
        _logger.LogInformation("No phases defined yet. Complete the planning stages first.");
        return Task.FromResult(0);
      }

      _logger.LogError("No phases found in the plan.");
      return Task.FromResult(1);
    }

    // Get the phase details file
    var planSubDir = _planManager.GetPlanSubDirectory(planName!);
    var phaseFileName = $"phase-{currentPhase.PhaseNumber}-details.md";
    var phaseFilePath = Path.Combine(planSubDir, phaseFileName);

    if (!_fileSystem.FileExists(phaseFilePath))
    {
      _logger.LogInformation("Phase {PhaseNumber} details file not found: {PhaseFileName}", currentPhase.PhaseNumber, phaseFileName);
      _logger.LogInformation("No verification requirements defined for this phase.");
      return Task.FromResult(0);
    }

    // Read the phase file content
    var phaseContent = _fileSystem.ReadAllText(phaseFilePath);

    // Look for the ### Verification section
    var verificationSectionStart = phaseContent.IndexOf("### Verification", StringComparison.OrdinalIgnoreCase);

    if (verificationSectionStart == -1)
    {
      _logger.LogInformation("Phase {PhaseNumber} has no verification section defined.", currentPhase.PhaseNumber);
      _logger.LogInformation("No verification requirements for this phase.");
      return Task.FromResult(0);
    }

    // Extract the verification section content (from ### Verification to the next ### or end of file)
    var verificationContent = phaseContent.Substring(verificationSectionStart);
    var nextSectionStart = verificationContent.IndexOf("\n###", 1, StringComparison.Ordinal); // Skip the first ### (Verification header)

    if (nextSectionStart != -1)
    {
      verificationContent = verificationContent.Substring(0, nextSectionStart);
    }

    // Output the verification instructions
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Verification requirements for phase {PhaseNumber} ({PhaseName}):", currentPhase.PhaseNumber, currentPhase.PhaseName);
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Please run all verification actions listed below and fix any errors found:");
    _logger.LogInformation(string.Empty);

    // Output the verification section content (without the ### Verification header)
    var lines = verificationContent.Split('\n').Skip(1); // Skip the ### Verification line
    foreach (var line in lines)
    {
      if (!string.IsNullOrWhiteSpace(line))
      {
        _logger.LogInformation(line);
      }
    }

    return Task.FromResult(0);
  }
}
