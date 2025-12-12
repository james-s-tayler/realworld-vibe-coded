using FlowPilot.Cli.Linting;
using FlowPilot.Cli.Models;
using Xunit;

namespace FlowPilot.Tests;

public class BranchPerPhaseLintingRuleTests
{
  [Fact]
  public async Task ExecuteAsync_WhenNotInPhaseDetailsStage_DoesNotAddError()
  {
    // Arrange
    var rule = new BranchPerPhaseLintingRule();
    var context = new PlanContext
    {
      CurrentBranch = "phase-1",
      State = new PlanState
      {
        HasPhaseDetails = false,
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Empty(context.LintingErrors);
  }

  [Fact]
  public async Task ExecuteAsync_WhenNoMorePhases_DoesNotAddError()
  {
    // Arrange
    var rule = new BranchPerPhaseLintingRule();
    var context = new PlanContext
    {
      CurrentBranch = "phase-3",
      State = new PlanState
      {
        HasPhaseDetails = true,
        Phases = new List<PhaseState>
        {
          new PhaseState { PhaseNumber = 1, IsComplete = true },
          new PhaseState { PhaseNumber = 2, IsComplete = true },
        },
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Empty(context.LintingErrors);
  }

  [Fact]
  public async Task ExecuteAsync_WhenTryingToAdvanceToNextPhaseOnOldBranch_AddsError()
  {
    // Arrange: Phase 1 is complete, trying to advance to phase 2 while on phase-1 branch
    var rule = new BranchPerPhaseLintingRule();
    var context = new PlanContext
    {
      CurrentBranch = "phase-1",
      State = new PlanState
      {
        HasPhaseDetails = true,
        Phases = new List<PhaseState>
        {
          new PhaseState { PhaseNumber = 1, IsComplete = true },
          new PhaseState { PhaseNumber = 2, IsComplete = false },
          new PhaseState { PhaseNumber = 3, IsComplete = false },
        },
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Single(context.LintingErrors);
    Assert.Contains("Cannot advance to phase 2 while on branch 'phase-1'", context.LintingErrors[0]);
    Assert.Contains("finish the current phase work", context.LintingErrors[0]);
    Assert.Contains("verification conditions are met", context.LintingErrors[0]);
    Assert.Contains("allow the PR to be reviewed and merged", context.LintingErrors[0]);
  }

  [Fact]
  public async Task ExecuteAsync_WhenOnBranchWithDescriptionAndAdvancingToNextPhase_AddsError()
  {
    // Arrange: Phase 1 complete, trying to advance to phase 2 on phase-1-implement-auth branch
    var rule = new BranchPerPhaseLintingRule();
    var context = new PlanContext
    {
      CurrentBranch = "phase-1-implement-auth",
      State = new PlanState
      {
        HasPhaseDetails = true,
        Phases = new List<PhaseState>
        {
          new PhaseState { PhaseNumber = 1, IsComplete = true },
          new PhaseState { PhaseNumber = 2, IsComplete = false },
        },
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Single(context.LintingErrors);
    Assert.Contains("Cannot advance to phase 2 while on branch 'phase-1-implement-auth'", context.LintingErrors[0]);
  }

  [Fact]
  public async Task ExecuteAsync_WhenAdvancingToCurrentBranchPhase_DoesNotAddError()
  {
    // Arrange: On phase-2 branch, advancing to phase 2 (first time on this branch)
    var rule = new BranchPerPhaseLintingRule();
    var context = new PlanContext
    {
      CurrentBranch = "phase-2",
      State = new PlanState
      {
        HasPhaseDetails = true,
        Phases = new List<PhaseState>
        {
          new PhaseState { PhaseNumber = 1, IsComplete = true },
          new PhaseState { PhaseNumber = 2, IsComplete = false },
        },
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Empty(context.LintingErrors);
  }

  [Fact]
  public async Task ExecuteAsync_WhenOnNonPhaseBranch_DoesNotAddError()
  {
    // Arrange
    var rule = new BranchPerPhaseLintingRule();
    var context = new PlanContext
    {
      CurrentBranch = "main",
      State = new PlanState
      {
        HasPhaseDetails = true,
        Phases = new List<PhaseState>
        {
          new PhaseState { PhaseNumber = 1, IsComplete = false },
        },
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Empty(context.LintingErrors);
  }

  [Fact]
  public async Task ExecuteAsync_WhenOnFeatureBranch_DoesNotAddError()
  {
    // Arrange
    var rule = new BranchPerPhaseLintingRule();
    var context = new PlanContext
    {
      CurrentBranch = "feature/my-feature",
      State = new PlanState
      {
        HasPhaseDetails = true,
        Phases = new List<PhaseState>
        {
          new PhaseState { PhaseNumber = 1, IsComplete = false },
        },
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Empty(context.LintingErrors);
  }

  [Fact]
  public async Task ExecuteAsync_WhenOnPhase1BranchAdvancingToPhase1_DoesNotAddError()
  {
    // Arrange: First time advancing to phase 1 on phase-1 branch
    var rule = new BranchPerPhaseLintingRule();
    var context = new PlanContext
    {
      CurrentBranch = "phase-1",
      State = new PlanState
      {
        HasPhaseDetails = true,
        Phases = new List<PhaseState>
        {
          new PhaseState { PhaseNumber = 1, IsComplete = false },
          new PhaseState { PhaseNumber = 2, IsComplete = false },
        },
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Empty(context.LintingErrors);
  }
}
