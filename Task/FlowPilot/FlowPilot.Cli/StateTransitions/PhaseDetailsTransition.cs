using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

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

  public PhaseDetailsTransition(
    PlanManager planManager,
    IFileSystemService fileSystem,
    TemplateService templateService,
    StateParser stateParser)
  {
    _planManager = planManager;
    _fileSystem = fileSystem;
    _templateService = templateService;
    _stateParser = stateParser;
  }

  public bool CanTransition(PlanContext context)
  {
    return context.State.HasPhaseAnalysis && !context.State.HasPhaseDetails;
  }

  public void Execute(PlanContext context)
  {
    // Parse phase-analysis.md to get phase names
    var phaseAnalysisPath = Path.Combine(context.MetaDirectory, "phase-analysis.md");
    var phaseNames = ParsePhaseNames(phaseAnalysisPath);

    if (phaseNames.Count == 0)
    {
      Console.WriteLine("Error: No phases found in phase-analysis.md");
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

    Console.WriteLine($"✓ Advanced to [phase-n-details] phase");
    Console.WriteLine($"✓ Created {phaseNames.Count} phase detail files");
    Console.WriteLine();
    Console.WriteLine("Instructions:");
    Console.WriteLine($"Update each phase-*-details.md file in .flowpilot/plans/{context.PlanName}/plan/");
    Console.WriteLine("based on the contents of goal.md, references.md, system-analysis.md,");
    Console.WriteLine("and phase-analysis.md.");
    Console.WriteLine();
    Console.WriteLine("⚠️  Note: A new branch is required to proceed to implementation.");
    Console.WriteLine("After committing, merge this branch before starting phase implementations.");
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
