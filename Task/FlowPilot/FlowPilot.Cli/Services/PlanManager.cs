using FlowPilot.Cli.Models;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Manages FlowPilot plan operations.
/// </summary>
public class PlanManager
{
  private readonly IFileSystemService _fileSystem;
  private readonly TemplateService _templateService;
  private readonly StateParser _stateParser;

  public PlanManager(IFileSystemService fileSystem, TemplateService templateService, StateParser stateParser)
  {
    _fileSystem = fileSystem;
    _templateService = templateService;
    _stateParser = stateParser;
  }

  public string GetPlanDirectory(string planName)
  {
    var rootDir = _fileSystem.GetCurrentDirectory();
    return Path.Combine(rootDir, ".flowpilot", "plans", planName);
  }

  public string GetMetaDirectory(string planName)
  {
    return Path.Combine(GetPlanDirectory(planName), "meta");
  }

  public string GetPlanSubDirectory(string planName)
  {
    return Path.Combine(GetPlanDirectory(planName), "plan");
  }

  public string GetStateFilePath(string planName)
  {
    return Path.Combine(GetMetaDirectory(planName), "state.md");
  }

  public bool PlanExists(string planName)
  {
    var stateFilePath = GetStateFilePath(planName);
    return _fileSystem.FileExists(stateFilePath);
  }

  public string GetPlansDirectory()
  {
    var rootDir = _fileSystem.GetCurrentDirectory();
    return Path.Combine(rootDir, ".flowpilot", "plans");
  }

  public List<string> GetAllPlans()
  {
    var plansDir = GetPlansDirectory();

    if (!_fileSystem.DirectoryExists(plansDir))
    {
      return new List<string>();
    }

    var planDirectories = _fileSystem.GetDirectories(plansDir);
    var planNames = new List<string>();

    foreach (var dir in planDirectories)
    {
      var planName = Path.GetFileName(dir);
      if (PlanExists(planName))
      {
        planNames.Add(planName);
      }
    }

    return planNames;
  }

  /// <summary>
  /// Resolves the plan name to use based on command arguments and available plans.
  /// Returns (planName, shouldExit, exitCode) tuple.
  /// </summary>
  public (string? PlanName, bool ShouldExit, int ExitCode) ResolvePlanName(string[] args, Action<string> logInfo, Action<string> logError)
  {
    var allPlans = GetAllPlans();

    // If no plans exist, exit successfully
    if (allPlans.Count == 0)
    {
      logInfo("No current plans. Successful exit.");
      return (null, true, 0);
    }

    string? planName = null;

    // If no argument provided, try to use the default plan
    if (args.Length == 0)
    {
      // If there's only one plan, use it as default
      if (allPlans.Count == 1)
      {
        planName = allPlans[0];
        logInfo($"Using default plan: {planName}");
      }
      else
      {
        // Multiple plans exist, require explicit plan name
        logError("Multiple plans exist. Please specify a plan name.");
        logInfo($"Available plans: {string.Join(", ", allPlans)}");
        return (null, true, 1);
      }
    }
    else
    {
      planName = args[0];

      if (!PlanExists(planName))
      {
        logError($"Plan '{planName}' not found");
        return (null, true, 1);
      }
    }

    return (planName, false, 0);
  }

  public PlanState GetCurrentState(string planName)
  {
    var stateFilePath = GetStateFilePath(planName);

    if (!_fileSystem.FileExists(stateFilePath))
    {
      return new PlanState();
    }

    var content = _fileSystem.ReadAllText(stateFilePath);
    var items = _stateParser.ParseStateFile(content);

    var state = new PlanState();

    foreach (var item in items)
    {
      switch (item.Identifier)
      {
        case "state":
          state.IsInitialized = item.IsChecked;
          break;
        case "references":
          state.HasReferences = item.IsChecked;
          break;
        case "system-analysis":
          state.HasSystemAnalysis = item.IsChecked;
          break;
        case "key-decisions":
          state.HasKeyDecisions = item.IsChecked;
          break;
        case "phase-analysis":
          state.HasPhaseAnalysis = item.IsChecked;
          break;
        case "phase-n-details":
          state.HasPhaseDetails = item.IsChecked;
          break;
        default:
          if (item.PhaseNumber.HasValue)
          {
            state.Phases.Add(new PhaseState
            {
              PhaseNumber = item.PhaseNumber.Value,
              PhaseName = item.Description,
              IsComplete = item.IsChecked,
            });
          }

          break;
      }
    }

    return state;
  }

  public void InitializePlan(string planName)
  {
    var metaDir = GetMetaDirectory(planName);

    if (!_fileSystem.DirectoryExists(metaDir))
    {
      _fileSystem.CreateDirectory(metaDir);
    }

    // Copy state.md template
    var stateTemplate = _templateService.ReadTemplate("state.md");
    var stateFilePath = GetStateFilePath(planName);

    // Mark [state] as checked
    var updatedState = _stateParser.UpdateChecklistItem(stateTemplate, "state", true);
    _fileSystem.WriteAllText(stateFilePath, updatedState);

    // Copy goal.md template
    var goalTemplate = _templateService.ReadTemplate("goal.md");
    var goalFilePath = Path.Combine(metaDir, "goal.md");
    _fileSystem.WriteAllText(goalFilePath, goalTemplate);
  }

  public void CopyTemplateToMeta(string planName, string templateName)
  {
    var template = _templateService.ReadTemplate(templateName);
    var metaDir = GetMetaDirectory(planName);
    var filePath = Path.Combine(metaDir, templateName);
    _fileSystem.WriteAllText(filePath, template);
  }

  public void UpdateStateChecklist(string planName, string identifier, bool isChecked)
  {
    var stateFilePath = GetStateFilePath(planName);
    var content = _fileSystem.ReadAllText(stateFilePath);
    var updatedContent = _stateParser.UpdateChecklistItem(content, identifier, isChecked);
    _fileSystem.WriteAllText(stateFilePath, updatedContent);
  }
}
