## phase_10: Update E2E Test Infrastructure for Per-Test Organization Registration

### Phase Overview

Update E2E test infrastructure to support parallel test execution by registering new organizations per-test instead of wiping the database. This enables tests to run in parallel without data interference while validating multi-tenancy semantics. Each test creates its own organization(s), ensuring complete isolation.

**Scope Size:** Small (~7 steps)
**Risk Level:** Low (test infrastructure improvements, no production code changes)
**Estimated Complexity:** Low

### Prerequisites

What must be completed before starting this phase:
- Phase 9 completed (handlers set TenantId on entity creation)
- System fully multi-tenant with data isolation
- All existing E2E tests passing

### Known Risks & Mitigations

**Risk 1:** Existing tests may break if database wipe is removed
- **Likelihood:** Medium
- **Impact:** Medium (test failures)
- **Mitigation:** Update existing tests incrementally, keep database wipe as fallback option initially
- **Fallback:** Keep database wipe logic but opt-in to new per-test org registration pattern

**Risk 2:** Test performance may degrade with more registrations
- **Likelihood:** Low
- **Impact:** Low (slightly longer test execution)
- **Mitigation:** Registration is fast, and parallelization will offset any slowdown
- **Fallback:** Accept slight performance trade-off for better parallelization

### Implementation Steps

**Part 1: Add Helper Methods for Per-Test Organization Registration**

1. **Add RegisterNewOrganizationUser helper to BasePage or test base class**
   - Add method that registers a new user with unique email/username (using GUID)
   - Each registration creates a new organization automatically (via phase 7 logic)
   - Return authentication token and user details
   - Expected outcome: Helper method for creating isolated test users
   - Files affected: `Test/e2e/E2eTests/Support/BasePage.cs` or `Test/e2e/E2eTests/AppPageTest.cs`
   - Reality check: Method compiles and can be called from tests

2. **Add helper for creating multiple orgs for cross-org isolation tests**
   - Add method that creates N users, each in separate organizations
   - Return collection of user contexts (auth tokens, user IDs, org IDs)
   - Expected outcome: Easy setup for cross-tenant isolation tests
   - Files affected: `Test/e2e/E2eTests/Support/BasePage.cs` or test base class
   - Reality check: Method compiles and can create multiple orgs

**Part 2: Update Existing Tests to Use Per-Test Org Registration**

3. **Identify tests that can use per-test org registration pattern**
   - Review existing E2E tests in `Test/e2e/E2eTests/`
   - Identify tests that test single-org functionality (articles, comments, follows, etc.)
   - List tests that need database wipe vs. per-test org registration
   - Expected outcome: Clear categorization of test update strategy
   - Files affected: None (analysis only)
   - Reality check: List of tests to update is clear

4. **Update article-related tests to use per-test org registration**
   - Replace database wipe setup with RegisterNewOrganizationUser calls
   - Each test creates its own user/org for test data
   - Verify tests can run in parallel without interference
   - Expected outcome: Article tests use per-test org pattern
   - Files affected: `Test/e2e/E2eTests/Article/*.cs`
   - Reality check: Run article tests with `./build.sh TestE2e` (filtered to Article tests)

5. **Update authentication and profile tests to use per-test org registration**
   - Replace database wipe with per-test org registration
   - Ensure registration/login tests create new orgs per-test
   - Expected outcome: Auth and profile tests use per-test org pattern
   - Files affected: `Test/e2e/E2eTests/Auth/*.cs`, `Test/e2e/E2eTests/Profile/*.cs`
   - Reality check: Run auth/profile tests with `./build.sh TestE2e` (filtered)

**Part 3: Enable Parallel Test Execution**

6. **Configure Playwright test runner for parallel execution**
   - Update test configuration to enable parallel test execution
   - Set appropriate worker count for CI/local environments
   - Verify tests can run in parallel without failures
   - Expected outcome: Tests can run in parallel
   - Files affected: `Test/e2e/E2eTests/playwright.config.cs` or similar
   - Reality check: Run full E2E suite with parallelization enabled

7. **Remove or deprecate database wipe logic**
   - Either remove database wipe entirely or mark as deprecated
   - Document new per-test org registration pattern as preferred approach
   - Expected outcome: Database wipe is no longer needed for test isolation
   - Files affected: `Test/e2e/E2eTests/Support/DatabaseHelper.cs` or similar
   - Reality check: Tests work without database wipe

### Reality Testing During Phase

**After Step 2:**
```bash
# Verify helper methods work
./build.sh LintServerVerify
./build.sh BuildServer
```

**After Step 5:**
```bash
# Run updated tests to verify per-test org registration works
./build.sh TestE2e
```

**After Step 6:**
```bash
# Verify parallel execution works
./build.sh TestE2e
# Check for race conditions or data interference
```

**After Step 7:**
```bash
# Run full E2E suite to ensure all tests pass
./build.sh TestE2e
```

### Expected Working State After Phase

- E2E tests create new organizations per-test instead of wiping database
- Tests can run in parallel without data interference
- Each test is isolated within its own organization(s)
- Test execution time improves due to parallelization
- Database wipe is no longer required for test isolation
- All existing E2E tests pass with new infrastructure

### If Phase Fails

**Debug First:**
1. Check test output for specific failures
2. Verify organization registration is working correctly in tests
3. Inspect database to ensure organizations are being created
4. Check for race conditions in parallel execution
5. Review logs in `Logs/Test/e2e/` for errors

**Common Issues:**
- Tests failing due to database wipe removal: Incrementally update tests, keep wipe as fallback
- Parallel execution causing race conditions: Reduce worker count or disable parallelization temporarily
- Registration helpers not working: Verify phase 7 registration logic is correct

**If stuck after several attempts:** Run `flowpilot stuck` for structured debugging guidance.

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintAllVerify
./build.sh BuildServer
./build.sh TestE2e
```

**Success Criteria:**
- All linting passes
- Server builds successfully
- All E2E tests pass with new per-test org registration pattern
- Tests can run in parallel without failures
- No database wipe required between tests

**Manual Verification (optional):**
1. Run E2E tests with parallelization enabled
2. Verify test execution time is reasonable
3. Check that tests create separate organizations
4. Confirm no data interference between parallel tests
