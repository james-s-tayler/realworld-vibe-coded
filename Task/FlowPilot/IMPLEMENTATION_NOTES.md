# PullRequestMergeBoundary Implementation Notes

## Summary

This PR implements the `PullRequestMergeBoundary` linting rule as specified in the issue. The rule enforces a branch-per-phase workflow by preventing more than one state.md checkbox change per branch.

## Implementation Details

### GitService Extensions
- Added `GetMergeBaseSha()` method to find the merge-base commit SHA between current branch and base branch (master/main)
- Added `CountChangedLines()` method to count line changes between two commits
- Added `CountStagedChangedLines()` method to count line changes in staged files

### PullRequestMergeBoundary Rule
- Compares state.md changes between merge-base and current HEAD (committed changes)
- Also counts changes in staged area (index vs HEAD)
- Allows at most 2 line changes total (one checkbox change = 1 deletion + 1 addition)
- If more than 2 changes detected, fails with the specified error message

### Registration
- Registered in `ServiceConfiguration.cs` as a transient `ILintingRule`

## Issue: Existing Tests Fail

The new linting rule causes the existing Docker integration tests (TEST 20-23) to fail. This is **BY DESIGN** - the tests are creating multiple phase changes on stacked branches without merging PRs between phases, which violates the branch-per-phase workflow that this rule enforces.

### Why Tests Fail

The existing test workflow:
1. Branch `master` → `phase-planning` (multiple planning phase commits)
2. Branch `phase-planning` → `phase-1-implementation` (phase implementation commit)
3. The linting rule compares `phase-1-implementation` HEAD to merge-base with `master`
4. This counts ALL state.md changes from both `phase-planning` AND `phase-1-implementation`
5. Result: More than 2 line changes detected → lint fails

### Solutions

Two approaches to fix:

1. **Update Tests to Follow Branch-Per-Phase Workflow**: Modify existing tests to:
   - Create PRs and merge them after each phase
   - Reset merge-base before continuing to next phase
   - This matches real-world workflow where each phase gets its own PR

2. **Make Rule Smarter for Stacked Branches**: Modify the rule to:
   - Detect stacked branch scenario
   - Only count changes since parent branch point, not since master
   - More complex but supports stacked PR workflow

## Recommendation

I recommend **Option 1** - update the tests to follow the intended branch-per-phase workflow. This better reflects real-world usage and validates that the rule works correctly in the intended workflow.

## Test Cases Specified in Issue

The issue requested 18 test cases covering:
- Planning phase (pass/fail)
- Boundary crossing (pass/fail)
- Implementation phase (pass/fail)
- Committed only, staged only, committed+staged variations

These tests would need to be added as new Docker integration tests that properly set up the branch/PR workflow.

## Next Steps

1. Decide on approach for handling existing tests
2. Implement comprehensive test suite per issue specification
3. Verify all tests pass
4. Request code review
