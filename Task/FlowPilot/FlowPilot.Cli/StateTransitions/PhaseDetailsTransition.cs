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
  private readonly PhaseAnalysisParser _phaseAnalysisParser;
  private readonly ILogger<PhaseDetailsTransition> _logger;

  public PhaseDetailsTransition(
    PlanManager planManager,
    IFileSystemService fileSystem,
    TemplateService templateService,
    StateParser stateParser,
    PhaseAnalysisParser phaseAnalysisParser,
    ILogger<PhaseDetailsTransition> logger)
  {
    _planManager = planManager;
    _fileSystem = fileSystem;
    _templateService = templateService;
    _stateParser = stateParser;
    _phaseAnalysisParser = phaseAnalysisParser;
    _logger = logger;
  }

  public bool CanTransition(PlanContext context)
  {
    return context.State.HasPhaseAnalysis && !context.State.HasPhaseDetails;
  }

  public void Execute(PlanContext context)
  {
    _logger.LogDebug("PhaseDetailsTransition.Execute called");

    // Parse phase-analysis.md to get phase information including PR boundaries
    var phaseAnalysisPath = Path.Combine(context.MetaDirectory, "phase-analysis.md");
    _logger.LogDebug("Parsing phases from {PhaseAnalysisPath}", phaseAnalysisPath);

    var phases = _phaseAnalysisParser.ParsePhases(phaseAnalysisPath);
    _logger.LogDebug("Found {Count} phases", phases.Count);

    if (phases.Count == 0)
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

    foreach (var phase in phases)
    {
      var fileName = $"phase-{phase.PhaseNumber}-details.md";
      var filePath = Path.Combine(context.PlanSubDirectory, fileName);
      var content = phaseTemplate.Replace("phase_n", phase.PhaseName);
      _fileSystem.WriteAllText(filePath, content);
    }

    // Update state.md to add phase checklist items and mark phase-n-details as checked
    var stateContent = _fileSystem.ReadAllText(context.StateFilePath);
    stateContent = _stateParser.UpdateChecklistItem(stateContent, "phase-n-details", true);

    // Add phase checklist items with PR boundary markers
    stateContent = AddPhaseChecklistItemsWithBoundaries(stateContent, phases);
    _fileSystem.WriteAllText(context.StateFilePath, stateContent);

    _logger.LogInformation("✓ Advanced to [phase-n-details] phase");
    _logger.LogInformation("✓ Created {PhaseCount} phase detail files", phases.Count);
    _logger.LogInformation(string.Empty);
    _logger.LogInformation("Instructions:");
    _logger.LogInformation("Update each phase-*-details.md file in .flowpilot/plans/{PlanName}/plan/", context.PlanName);
    _logger.LogInformation("based on the contents of goal.md, references.md, system-analysis.md,");
    _logger.LogInformation("and phase-analysis.md.");
    _logger.LogInformation(string.Empty);
    _logger.LogWarning("A new branch is required to proceed to implementation.");
    _logger.LogInformation("After committing, merge this branch before starting phase implementations.");
  }

  private string AddPhaseChecklistItemsWithBoundaries(string content, List<PhaseInfo> phases)
  {
    var lines = content.Split('\n').ToList();

    // Find the position after [phase-n-details]
    var detailsIndex = lines.FindIndex(l => l.Contains("[phase-n-details]"));
    if (detailsIndex == -1)
    {
      return content;
    }

    // Add phase items after the phase-n-details line
    for (int i = 0; i < phases.Count; i++)
    {
      var phase = phases[i];
      var prBoundaryMarker = phase.IsPullRequestBoundary ? " [PR-BOUNDARY]" : string.Empty;
      var phaseItem = $"- [ ] [phase_{phase.PhaseNumber}] {phase.PhaseName}{prBoundaryMarker}";
      lines.Insert(detailsIndex + 1 + i, phaseItem);
    }

    return string.Join('\n', lines);
  }
}
