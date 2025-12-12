using FlowPilot.Cli.Linting;
using FlowPilot.Cli.Models;
using Xunit;

namespace FlowPilot.Tests;

public class PullRequestBoundaryLintingRuleTests
{
  [Fact]
  public async Task ExecuteAsync_WhenNotInPhaseDetailsStage_DoesNotAddError()
  {
    // Arrange
    var rule = new PullRequestBoundaryLintingRule();
    var context = new PlanContext
    {
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
    var rule = new PullRequestBoundaryLintingRule();
    var context = new PlanContext
    {
      State = new PlanState
      {
        HasPhaseDetails = true,
        Phases = new List<PhaseState>
        {
          new PhaseState { PhaseNumber = 1, IsComplete = true, IsPullRequestBoundary = false },
          new PhaseState { PhaseNumber = 2, IsComplete = true, IsPullRequestBoundary = true },
        },
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Empty(context.LintingErrors);
  }

  [Fact]
  public async Task ExecuteAsync_WhenTryingToAdvanceBeyondPrBoundary_AddsError()
  {
    // Arrange
    var rule = new PullRequestBoundaryLintingRule();
    var context = new PlanContext
    {
      State = new PlanState
      {
        HasPhaseDetails = true,
        Phases = new List<PhaseState>
        {
          new PhaseState { PhaseNumber = 1, PhaseName = "Setup", IsComplete = true, IsPullRequestBoundary = false },
          new PhaseState { PhaseNumber = 2, PhaseName = "Database", IsComplete = true, IsPullRequestBoundary = true },
          new PhaseState { PhaseNumber = 3, PhaseName = "API", IsComplete = false, IsPullRequestBoundary = false },
        },
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Single(context.LintingErrors);
    Assert.Contains("Cannot advance to phase 3", context.LintingErrors[0]);
    Assert.Contains("Phase 2 (Database) is a PR boundary", context.LintingErrors[0]);
    Assert.Contains("ensure all verification conditions are met", context.LintingErrors[0]);
    Assert.Contains("allow the PR to be reviewed and merged", context.LintingErrors[0]);
  }

  [Fact]
  public async Task ExecuteAsync_WhenNoPrBoundaryCompleted_DoesNotAddError()
  {
    // Arrange
    var rule = new PullRequestBoundaryLintingRule();
    var context = new PlanContext
    {
      State = new PlanState
      {
        HasPhaseDetails = true,
        Phases = new List<PhaseState>
        {
          new PhaseState { PhaseNumber = 1, IsComplete = true, IsPullRequestBoundary = false },
          new PhaseState { PhaseNumber = 2, IsComplete = false, IsPullRequestBoundary = false },
        },
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Empty(context.LintingErrors);
  }

  [Fact]
  public async Task ExecuteAsync_WhenAdvancingToFirstPhaseAfterPrBoundary_DoesNotAddError()
  {
    // Arrange: Phase 2 is PR boundary but not complete yet, advancing to phase 2
    var rule = new PullRequestBoundaryLintingRule();
    var context = new PlanContext
    {
      State = new PlanState
      {
        HasPhaseDetails = true,
        Phases = new List<PhaseState>
        {
          new PhaseState { PhaseNumber = 1, IsComplete = true, IsPullRequestBoundary = false },
          new PhaseState { PhaseNumber = 2, IsComplete = false, IsPullRequestBoundary = true },
          new PhaseState { PhaseNumber = 3, IsComplete = false, IsPullRequestBoundary = false },
        },
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Empty(context.LintingErrors);
  }

  [Fact]
  public async Task ExecuteAsync_WithMultiplePrBoundaries_ChecksLastOne()
  {
    // Arrange: Multiple PR boundaries, last completed one should trigger error
    var rule = new PullRequestBoundaryLintingRule();
    var context = new PlanContext
    {
      State = new PlanState
      {
        HasPhaseDetails = true,
        Phases = new List<PhaseState>
        {
          new PhaseState { PhaseNumber = 1, IsComplete = true, IsPullRequestBoundary = true },
          new PhaseState { PhaseNumber = 2, IsComplete = true, IsPullRequestBoundary = false },
          new PhaseState { PhaseNumber = 3, IsComplete = true, IsPullRequestBoundary = true },
          new PhaseState { PhaseNumber = 4, IsComplete = false, IsPullRequestBoundary = false },
        },
      },
    };

    // Act
    await rule.ExecuteAsync(context);

    // Assert
    Assert.Single(context.LintingErrors);
    Assert.Contains("Phase 3", context.LintingErrors[0]);
  }
}
