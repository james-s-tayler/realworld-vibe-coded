# PullRequestMergeBoundary Implementation Notes

## Summary

This PR implements the `PullRequestMergeBoundary` linting rule as specified in the issue. The rule enforces a branch-per-phase workflow by preventing more than one state.md checkbox change per branch.

## Implementation Details

### GitService Extensions
- Added `GetMergeBaseSha()` method to find the merge-base commit SHA between current branch and base branch (master/main)
- Added `GetHeadSha()` method to get current HEAD commit SHA
- Added `CountChangedLines()` method to count line changes between two commits
- Added `CountStagedChangedLines()` method to count line changes in staged files
- Added `CountChangedLinesInPatch()` helper method to reduce code duplication

### PullRequestMergeBoundary Rule
- Compares state.md changes between merge-base and current HEAD (committed changes)
- Also counts changes in staged area (index vs HEAD)
- Allows at most 2 line changes total (one checkbox change = 1 deletion + 1 addition)
- If more than 2 changes detected, fails with the specified error message

### Registration
- Registered in `ServiceConfiguration.cs` as a transient `ILintingRule`

## Tests Updated

The existing Docker integration tests (TEST 1-25) have been updated to follow the branch-per-phase workflow:

- **TEST 15-19**: Planning phase tests now properly branch and commit
- **TEST 19b**: Added PR merge simulation - merges `phase-planning` branch back to `master`
- **TEST 20-21**: Phase 1 implementation on new branch, then merged to master
- **TEST 22-23**: Phase 2 implementation on new branch, then merged to master
- **TEST 24-25**: Phase 3 implementation and completion

### Key Changes

1. After completing the planning phase (TEST 19), the `phase-planning` branch is merged back to master
2. Each implementation phase (1, 2, 3) starts from a new branch off master
3. Each phase is merged back to master after completion
4. This resets the merge-base between phases, allowing the `PullRequestMergeBoundary` rule to correctly validate only one state change per branch

### Why This Works

By merging each phase back to master:
- The merge-base moves forward to include all previous phase changes
- When a new branch is created for the next phase, it starts from this updated merge-base
- The `PullRequestMergeBoundary` rule only counts changes since the new merge-base
- This allows exactly one state.md checkbox change per branch/PR

## Test Results

All 25 tests now pass successfully:
```
✅ ALL TESTS PASSED (25/25)

Summary:
  - Basic commands: init, new, help
  - Linting validation
  - State transitions through all phases
  - Hard boundary enforcement with PR merges
  - Template file creation
  - Complete plan workflow
```

