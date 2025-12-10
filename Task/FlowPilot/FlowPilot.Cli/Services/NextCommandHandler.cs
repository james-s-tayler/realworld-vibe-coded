using FlowPilot.Cli.Models;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Handles the logic for the 'next' command.
/// </summary>
public class NextCommandHandler
{
  private readonly PlanManager _planManager;
  private readonly IFileSystemService _fileSystem;
  private readonly TemplateService _templateService;
  private readonly StateParser _stateParser;

  public NextCommandHandler(
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

  public void Execute(string planName)
  {
    var state = _planManager.GetCurrentState(planName);

    if (!state.IsInitialized)
    {
      Console.WriteLine("Error: Plan not initialized. Run 'flowpilot init' first.");
      Environment.Exit(1);
      return;
    }

    // State machine logic
    if (!state.HasReferences)
    {
      AdvanceToReferences(planName);
    }
    else if (!state.HasSystemAnalysis)
    {
      AdvanceToSystemAnalysis(planName);
    }
    else if (!state.HasKeyDecisions)
    {
      AdvanceToKeyDecisions(planName);
    }
    else if (!state.HasPhaseAnalysis)
    {
      AdvanceToPhaseAnalysis(planName);
    }
    else if (!state.HasPhaseDetails)
    {
      AdvanceToPhaseDetails(planName);
    }
    else
    {
      AdvanceToNextPhase(planName, state);
    }
  }

  private void AdvanceToReferences(string planName)
  {
    _planManager.UpdateStateChecklist(planName, "references", true);
    _planManager.CopyTemplateToMeta(planName, "references.md");

    Console.WriteLine("✓ Advanced to [references] phase");
    Console.WriteLine();
    Console.WriteLine("Instructions:");
    Console.WriteLine($"Update .flowpilot/plans/{planName}/meta/references.md");
    Console.WriteLine("Use the mslearn MCP server and web search to conduct thorough research");
    Console.WriteLine("based on the goal stated in goal.md. Document your findings in references.md");
    Console.WriteLine("to aid the implementation plan.");
  }

  private void AdvanceToSystemAnalysis(string planName)
  {
    _planManager.UpdateStateChecklist(planName, "system-analysis", true);
    _planManager.CopyTemplateToMeta(planName, "system-analysis.md");

    Console.WriteLine("✓ Advanced to [system-analysis] phase");
    Console.WriteLine();
    Console.WriteLine("Instructions:");
    Console.WriteLine($"Update .flowpilot/plans/{planName}/meta/system-analysis.md");
    Console.WriteLine("Analyze the current parts of the system that are relevant to the goal");
    Console.WriteLine("stated in goal.md and record them in system-analysis.md.");
  }

  private void AdvanceToKeyDecisions(string planName)
  {
    _planManager.UpdateStateChecklist(planName, "key-decisions", true);
    _planManager.CopyTemplateToMeta(planName, "key-decisions.md");

    Console.WriteLine("✓ Advanced to [key-decisions] phase");
    Console.WriteLine();
    Console.WriteLine("Instructions:");
    Console.WriteLine($"Update .flowpilot/plans/{planName}/meta/key-decisions.md");
    Console.WriteLine("Document any decisions that need to be made based on the contents of");
    Console.WriteLine("goal.md, references.md, and system-analysis.md.");
    Console.WriteLine();
    Console.WriteLine("⚠️  Note: A new branch is required to proceed past key-decisions.");
    Console.WriteLine("After committing, merge this branch before continuing with phase-analysis.");
  }

  private void AdvanceToPhaseAnalysis(string planName)
  {
    _planManager.UpdateStateChecklist(planName, "phase-analysis", true);
    _planManager.CopyTemplateToMeta(planName, "phase-analysis.md");

    Console.WriteLine("✓ Advanced to [phase-analysis] phase");
    Console.WriteLine();
    Console.WriteLine("Instructions:");
    Console.WriteLine($"Update .flowpilot/plans/{planName}/meta/phase-analysis.md");
    Console.WriteLine("Based on the contents of goal.md, system-analysis.md, and key-decisions.md,");
    Console.WriteLine("define the high-level phases for this plan.");
    Console.WriteLine();
    Console.WriteLine("⚠️  Note: A new branch is required to proceed past phase-analysis.");
    Console.WriteLine("After committing, merge this branch before continuing with phase-details.");
  }

  private void AdvanceToPhaseDetails(string planName)
  {
    // Parse phase-analysis.md to get phase names
    var phaseAnalysisPath = Path.Combine(_planManager.GetMetaDirectory(planName), "phase-analysis.md");
    var phaseNames = ParsePhaseNames(phaseAnalysisPath);

    if (phaseNames.Count == 0)
    {
      Console.WriteLine("Error: No phases found in phase-analysis.md");
      Environment.Exit(1);
      return;
    }

    // Create phase detail files
    var planDir = _planManager.GetPlanSubDirectory(planName);

    if (!_fileSystem.DirectoryExists(planDir))
    {
      _fileSystem.CreateDirectory(planDir);
    }

    var phaseTemplate = _templateService.ReadTemplate("phase-n-details.md");

    for (int i = 0; i < phaseNames.Count; i++)
    {
      var fileName = $"phase-{i + 1}-details.md";
      var filePath = Path.Combine(planDir, fileName);
      var content = phaseTemplate.Replace("phase_n", $"phase_{i + 1}");
      _fileSystem.WriteAllText(filePath, content);
    }

    // Update state.md to add phase checklist items and mark phase-n-details as checked
    var stateFilePath = _planManager.GetStateFilePath(planName);
    var stateContent = _fileSystem.ReadAllText(stateFilePath);
    stateContent = _stateParser.UpdateChecklistItem(stateContent, "phase-n-details", true);
    stateContent = _stateParser.AddPhaseChecklistItems(stateContent, phaseNames);
    _fileSystem.WriteAllText(stateFilePath, stateContent);

    Console.WriteLine($"✓ Advanced to [phase-n-details] phase");
    Console.WriteLine($"✓ Created {phaseNames.Count} phase detail files");
    Console.WriteLine();
    Console.WriteLine("Instructions:");
    Console.WriteLine($"Update each phase-*-details.md file in .flowpilot/plans/{planName}/plan/");
    Console.WriteLine("based on the contents of goal.md, references.md, system-analysis.md,");
    Console.WriteLine("and phase-analysis.md.");
    Console.WriteLine();
    Console.WriteLine("⚠️  Note: A new branch is required to proceed to implementation.");
    Console.WriteLine("After committing, merge this branch before starting phase implementations.");
  }

  private void AdvanceToNextPhase(string planName, PlanState state)
  {
    // Find the next uncompleted phase
    var nextPhase = state.Phases.FirstOrDefault(p => !p.IsComplete);

    if (nextPhase == null)
    {
      Console.WriteLine("✓ All phases complete! Plan finished.");
      return;
    }

    _planManager.UpdateStateChecklist(planName, $"phase_{nextPhase.PhaseNumber}", true);

    Console.WriteLine($"✓ Advanced to phase {nextPhase.PhaseNumber}: {nextPhase.PhaseName}");
    Console.WriteLine();
    Console.WriteLine("Instructions:");
    Console.WriteLine($"Implement phase {nextPhase.PhaseNumber} as described in:");
    Console.WriteLine($".flowpilot/plans/{planName}/plan/phase-{nextPhase.PhaseNumber}-details.md");
    Console.WriteLine();
    Console.WriteLine("When phase verification criteria is met, run 'flowpilot next' again.");
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
