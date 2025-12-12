using FlowPilot.Cli.Models;

namespace FlowPilot.Cli.Linting;

/// <summary>
/// Validates that phase advancement only happens when on a new branch.
/// </summary>
public class BranchPerPhaseLintingRule : ILintingRule
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

    // Extract phase number from current branch name
    // Expected format: phase-N or phase-N-description
    var branchName = context.CurrentBranch;
    var branchPhaseNumber = ExtractPhaseNumberFromBranch(branchName);

    // If we're not on a phase-numbered branch, allow advancement
    if (!branchPhaseNumber.HasValue)
    {
      return Task.CompletedTask;
    }

    // If the branch phase number is less than the next phase we're advancing to,
    // that means we've already completed work on this branch's phase and are trying
    // to advance to a later phase. This is not allowed - user must create a new branch.
    if (branchPhaseNumber.Value < nextPhase.PhaseNumber)
    {
      context.LintingErrors.Add(
        $"Cannot advance to phase {nextPhase.PhaseNumber} while on branch '{branchName}'. " +
        $"You must finish the current phase work, ensure all verification conditions are met, " +
        $"and allow the PR to be reviewed and merged before moving to the next phase. " +
        $"Create a new branch for phase {nextPhase.PhaseNumber} before running 'flowpilot next' again.");
    }

    return Task.CompletedTask;
  }

  private static int? ExtractPhaseNumberFromBranch(string branchName)
  {
    // Match patterns like: phase-1, phase-2-description, etc.
    var match = System.Text.RegularExpressions.Regex.Match(branchName, @"^phase-(\d+)(?:-|$)");

    if (match.Success && int.TryParse(match.Groups[1].Value, out int phaseNumber))
    {
      return phaseNumber;
    }

    return null;
  }
}
