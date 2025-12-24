## phase_9

### Phase Overview

Brief description of what this phase accomplishes (1-2 sentences)

**Scope Size:** Small/Medium/Large
**Risk Level:** Low/Medium/High
**Estimated Complexity:** Low/Medium/High

### Prerequisites

What must be completed before starting this phase:
- Prerequisite 1
- Prerequisite 2
- All tests from previous phase passing

### Known Risks & Mitigations

**Risk 1:** Description
- **Likelihood:** High/Medium/Low
- **Impact:** High/Medium/Low
- **Mitigation:** How we plan to avoid or handle this
- **Fallback:** What to do if the risk materializes

**Risk 2:** Description
- **Likelihood:** High/Medium/Low
- **Impact:** High/Medium/Low
- **Mitigation:** How we plan to avoid or handle this
- **Fallback:** What to do if the risk materializes

### Implementation Steps

**Part 1: Setup & Preparation**

1. **Step Title**
   - Detailed action to take
   - Expected outcome
   - Files affected: `path/to/file1.cs`, `path/to/file2.cs`

2. **Step Title**
   - Detailed action to take
   - Expected outcome

**Part 2: Core Changes**

3. **Step Title**
   - Detailed action to take
   - Expected outcome
   - Reality check: How to verify this step worked

**Part 3: Testing & Validation**

4. **Run tests incrementally**
   - Run `./build.sh TestX` after each major change
   - Fix issues immediately before proceeding
   - Don't accumulate broken tests

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After backend changes
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer

# After test changes
./build.sh TestServerPostman
./build.sh TestE2e
```

Don't wait until the end to test. Reality test after each major change.

### Expected Working State After Phase

Describe what should be true when this phase is complete:
- Application builds successfully
- Specific feature X works as expected
- All tests pass
- Database schema is in state Y
- API endpoints respond as expected

### If Phase Fails

If this phase fails and cannot be completed:
1. Try to do a debug analysis using `debug-analysis.md` to help identify the root cause
2. If the debug analysis doesn't resolve the issue, run `flowpilot stuck` to get assistance with re-planning

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintAllVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass before proceeding to the next phase.

**Manual Verification Steps:**
1. Start the application and verify it runs
2. Test specific functionality X manually
3. Check logs for errors
4. Verify database state is correct