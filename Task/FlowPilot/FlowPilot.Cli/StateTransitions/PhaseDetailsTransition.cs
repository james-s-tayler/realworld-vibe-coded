using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.StateTransitions;

/// <summary>
/// Transition to the phase-details phase.
/// </summary>
public class PhaseDetailsTransition : IStateTransition
{
  private readonly PlanManager _planManager;
  private readonly IFileSystemService _fileSystem;
  private readonly TemplateService _templateService;
  private readonly StateParser _stateParser;
  private readonly ILogger<PhaseDetailsTransition> _logger;

  public PhaseDetailsTransition(
    PlanManager planManager,
    IFileSystemService fileSystem,
    TemplateService templateService,
    StateParser stateParser,
    ILogger<PhaseDetailsTransition> logger)
  {
    _planManager = planManager;
    _fileSystem = fileSystem;
    _templateService = templateService;
    _stateParser = stateParser;
    _logger = logger;
  }

  public bool CanTransition(PlanContext context)
  {
    return context.State.HasPhaseAnalysis && !context.State.HasPhaseDetails;
  }

  public void Execute(PlanContext context)
  {
    _logger.LogDebug("PhaseDetailsTransition.Execute called");

    // Parse phase-analysis.md to get phase names
    var phaseAnalysisPath = Path.Combine(context.MetaDirectory, "phase-analysis.md");
    _logger.LogDebug("Parsing phase names from {PhaseAnalysisPath}", phaseAnalysisPath);

    var phaseNames = ParsePhaseNames(phaseAnalysisPath);
    _logger.LogDebug("Found {Count} phases: {PhaseNames}", phaseNames.Count, string.Join(", ", phaseNames));

    if (phaseNames.Count == 0)
    {
      _logger.LogError("No phases found in phase-analysis.md");
      Environment.Exit(1);
      return;
    }

    // Create phase detail files
    if (!_fileSystem.DirectoryExists(context.PlanSubDirectory))
    {
      _fileSystem.CreateDirectory(context.PlanSubDirectory);
    }

    var phaseTemplate = _templateService.ReadTemplate("phase-n-details.md");

    for (int i = 0; i < phaseNames.Count; i++)
    {
      var fileName = $"phase-{i + 1}-details.md";
      var filePath = Path.Combine(context.PlanSubDirectory, fileName);
      var content = phaseTemplate.Replace("phase_n", $"phase_{i + 1}");
      _fileSystem.WriteAllText(filePath, content);
    }

    // Update state.md to add phase checklist items and mark phase-n-details as checked
    var stateContent = _fileSystem.ReadAllText(context.StateFilePath);
    stateContent = _stateParser.UpdateChecklistItem(stateContent, "phase-n-details", true);
    stateContent = _stateParser.AddPhaseChecklistItems(stateContent, phaseNames);
    _fileSystem.WriteAllText(context.StateFilePath, stateContent);

    _logger.LogInformation("✓ Advanced to [phase-n-details] phase");
    _logger.LogInformation("✓ Created {PhaseCount} phase detail files", phaseNames.Count);
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Instructions:");
    _logger.LogInformation("Update each phase-*-details.md file in .flowpilot/plans/{PlanName}/plan/", context.PlanName);
    _logger.LogInformation("based on the contents of goal.md, references.md, system-analysis.md,");
    _logger.LogInformation("and phase-analysis.md.");
    _logger.LogInformation(string.Empty);
    _logger.LogWarning("A new branch is required to proceed to implementation.");
    _logger.LogInformation("After committing, merge this branch before starting phase implementations.");
  }

  private List<string> ParsePhaseNames(string phaseAnalysisPath)
  {
    var phaseNames = new List<string>();

    if (!_fileSystem.FileExists(phaseAnalysisPath))
    {
      return phaseNames;
    }

    var content = _fileSystem.ReadAllText(phaseAnalysisPath);
    var lines = content.Split('\n');

    foreach (var line in lines)
    {
      // Look for phase headers: ### phase_N
      if (line.StartsWith("### phase_", StringComparison.Ordinal))
      {
        var phaseName = line.Substring(4).Trim();
        phaseNames.Add(phaseName);
      }
    }

    return phaseNames;
  }
}
