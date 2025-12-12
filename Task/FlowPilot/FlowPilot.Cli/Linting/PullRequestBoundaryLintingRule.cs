using FlowPilot.Cli.Models;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that phase advancement respects PR boundaries.
/// When a phase is marked as a PR boundary, the next phase cannot be started
/// until the current PR is reviewed and merged.
/// </summary>
public class PullRequestBoundaryLintingRule : ILintingRule
{
  public Task ExecuteAsync(PlanContext context)
  {
    // Only enforce this rule if we're in phase implementation stage
    if (!context.State.HasPhaseDetails)
    {
      return Task.CompletedTask;
    }

    // Find the next uncompleted phase (the one we would advance to)
    var nextPhase = context.State.Phases.FirstOrDefault(p => !p.IsComplete);

    if (nextPhase == null)
    {
      // No more phases to complete, no need to check
      return Task.CompletedTask;
    }

    // Check if any completed phases before the next one are PR boundaries
    // This would indicate we're trying to advance beyond a PR boundary without merging
    var completedPhases = context.State.Phases
      .Where(p => p.PhaseNumber < nextPhase.PhaseNumber && p.IsComplete)
      .ToList();

    // Find the last completed PR boundary phase
    var lastPrBoundary = completedPhases
      .Where(p => p.IsPullRequestBoundary)
      .OrderByDescending(p => p.PhaseNumber)
      .FirstOrDefault();

    if (lastPrBoundary != null)
    {
      // There's a completed PR boundary phase before the next phase
      // This means we're in the same PR as that boundary was completed
      // and trying to advance beyond it, which is not allowed
      context.LintingErrors.Add(
        $"Cannot advance to phase {nextPhase.PhaseNumber} ({nextPhase.PhaseName}) in the same pull request. " +
        $"Phase {lastPrBoundary.PhaseNumber} ({lastPrBoundary.PhaseName}) is a PR boundary. " +
        $"You must finish the current phase work, ensure all verification conditions are met, " +
        $"and allow the PR to be reviewed and merged before moving to the next phase. " +
        $"After the PR is merged, create a new issue/PR and run 'flowpilot next' to continue.");
    }

    return Task.CompletedTask;
  }
}
