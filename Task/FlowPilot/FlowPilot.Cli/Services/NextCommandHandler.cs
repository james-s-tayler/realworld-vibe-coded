using FlowPilot.Cli.Models;
using FlowPilot.Cli.StateTransitions;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Handles the logic for the 'next' command using discrete state transitions.
/// </summary>
public class NextCommandHandler
{
  private readonly PlanManager _planManager;
  private readonly LintCommandHandler _lintCommandHandler;
  private readonly GitService _gitService;
  private readonly IEnumerable<IStateTransition> _stateTransitions;
  private readonly ILogger<NextCommandHandler> _logger;

  public NextCommandHandler(
    PlanManager planManager,
    LintCommandHandler lintCommandHandler,
    GitService gitService,
    IEnumerable<IStateTransition> stateTransitions,
    ILogger<NextCommandHandler> logger)
  {
    _planManager = planManager;
    _lintCommandHandler = lintCommandHandler;
    _gitService = gitService;
    _stateTransitions = stateTransitions;
    _logger = logger;
  }

  public async Task ExecuteAsync(string planName)
  {
    _logger.LogInformation("NextCommandHandler.ExecuteAsync called for plan: {PlanName}", planName);

    var state = _planManager.GetCurrentState(planName);
    _logger.LogDebug(
      "Current state: HasPhaseAnalysis={HasPhaseAnalysis}, HasPhaseDetails={HasPhaseDetails}, IsInitialized={IsInitialized}, PhaseCount={PhaseCount}",
      state.HasPhaseAnalysis,
      state.HasPhaseDetails,
      state.IsInitialized,
      state.Phases.Count);

    if (!state.IsInitialized)
    {
      _logger.LogError("Plan not initialized. Run 'flowpilot new' first");
      Environment.Exit(1);
      return;
    }

    // Build plan context
    _logger.LogDebug("Building plan context");
    var context = new PlanContext
    {
      PlanName = planName,
      State = state,
      PlanDirectory = _planManager.GetPlanDirectory(planName),
      MetaDirectory = _planManager.GetMetaDirectory(planName),
      PlanSubDirectory = _planManager.GetPlanSubDirectory(planName),
      StateFilePath = _planManager.GetStateFilePath(planName),
    };

    _logger.LogDebug(
      "Plan context built: PlanDirectory={PlanDirectory}, StateFilePath={StateFilePath}",
      context.PlanDirectory,
      context.StateFilePath);

    // Find the first applicable transition
    _logger.LogDebug("Searching for applicable state transition among {Count} transitions", _stateTransitions.Count());
    var transition = _stateTransitions.FirstOrDefault(t =>
    {
      var canTransition = t.CanTransition(context);
      _logger.LogDebug(
        "Transition {TransitionType} CanTransition: {CanTransition}",
        t.GetType().Name,
        canTransition);
      return canTransition;
    });

    if (transition != null)
    {
      _logger.LogInformation("Found applicable transition: {TransitionType}", transition.GetType().Name);
      _logger.LogDebug("Executing transition: {TransitionType}", transition.GetType().Name);

      // Save the original state.md content before transition
      _logger.LogDebug("Reading original state file content");
      var originalStateContent = File.ReadAllText(context.StateFilePath);

      // Execute the state transition (which updates state.md and creates template files)
      _logger.LogInformation("Executing state transition");
      transition.Execute(context);
      _logger.LogDebug("State transition executed");

      // Save the new state.md content after transition
      _logger.LogDebug("Reading new state file content");
      var newStateContent = File.ReadAllText(context.StateFilePath);

      // Get the relative path to state.md for git operations
      var repositoryRoot = _gitService.GetRepositoryRoot();
      var relativeStatePath = Path.GetRelativePath(repositoryRoot, context.StateFilePath);
      _logger.LogDebug("Relative state path: {RelativeStatePath}", relativeStatePath);

      // Stage the new state content so PullRequestMergeBoundary can see it
      _logger.LogDebug("Staging state file");
      _gitService.StageFile(relativeStatePath);

      // Also stage any newly created template files in the plan directory
      var planDirectoryRelative = Path.GetRelativePath(repositoryRoot, context.PlanDirectory);
      var changedFiles = _gitService.GetChangedFiles();
      _logger.LogDebug("Found {ChangedFilesCount} changed files in repository", changedFiles.Count);

      var stagedPlanFiles = new List<string>();
      foreach (var changedFile in changedFiles)
      {
        // Stage files in the plan directory (meta/ and plan/ subdirectories)
        if (changedFile.StartsWith(planDirectoryRelative, StringComparison.OrdinalIgnoreCase) &&
            changedFile != relativeStatePath)
        {
          _logger.LogDebug("Staging plan file: {ChangedFile}", changedFile);
          _gitService.StageFile(changedFile);
          stagedPlanFiles.Add(changedFile);
        }
      }

      _logger.LogDebug("Staged {StagedCount} plan files", stagedPlanFiles.Count);

      // Temporarily restore old state content on disk so template rules see the old state
      // This prevents template rules from checking templates that were just created
      _logger.LogDebug("Temporarily restoring original state content for lint validation");
      File.WriteAllText(context.StateFilePath, originalStateContent);

      // Now run lint to validate the transition
      // Template rules will see the old state (from disk)
      // PullRequestMergeBoundary will see the new state (from staged area)
      _logger.LogInformation(string.Empty);
      _logger.LogInformation("Running lint validation...");
      var lintResult = await _lintCommandHandler.ExecuteAsync(planName);
      _logger.LogDebug("Lint result: {LintResult}", lintResult);

      if (lintResult != 0)
      {
        // Lint failed - keep the original state and unstage all changes
        _logger.LogError("Lint failed. Reverting state transition.");
        _logger.LogDebug("Resetting staged state file and template files");

        // state.md already has original content, just unstage it from both working tree and index
        _gitService.ResetFile(relativeStatePath);

        // Also reset all staged plan files (template files created during transition)
        foreach (var stagedFile in stagedPlanFiles)
        {
          _logger.LogDebug("Resetting staged plan file: {StagedFile}", stagedFile);
          _gitService.ResetFile(stagedFile);
        }

        Environment.Exit(1);
        return;
      }

      // Lint passed - restore the new state content on disk
      _logger.LogDebug("Lint passed, restoring new state content");
      File.WriteAllText(context.StateFilePath, newStateContent);
      _logger.LogInformation("State transition completed successfully");
    }
    else
    {
      _logger.LogDebug("No applicable state transition found");

      // Check if plan is complete
      if (state.Phases.Any() && state.Phases.All(p => p.IsComplete))
      {
        _logger.LogInformation("✓ Plan '{PlanName}' is complete!", planName);
        _logger.LogInformation("All phases have been finished.");
      }
      else
      {
        _logger.LogError("No applicable state transition found");
        Environment.Exit(1);
      }
    }
  }
}
