## phase_10: Add E2E Tests for Multi-Tenancy and Cross-Tenant Isolation

### Phase Overview

Add comprehensive E2E tests using Playwright to verify multi-tenancy semantics. Test that users in different organizations cannot see each other's data. Verify articles, comments, tags, and follow relationships are properly scoped to tenants. Update database wipe scripts to handle Organizations table.

**Scope Size:** Small (~8 steps)
**Risk Level:** Low (additive test coverage)
**Estimated Complexity:** Low

### Prerequisites

What must be completed before starting this phase:
- Phase 9 completed (handlers set TenantId on entity creation)
- System fully multi-tenant with data isolation
- All existing tests passing

### Known Risks & Mitigations

**Risk 1:** E2E tests may be slow with multiple user registrations
- **Likelihood:** Medium
- **Impact:** Low (test execution time)
- **Mitigation:** Use efficient test patterns, minimize redundant setup
- **Fallback:** Accept slower tests for comprehensive coverage

**Risk 2:** Database wipe scripts may not handle Organizations correctly
- **Likelihood:** Medium
- **Impact:** Medium (test isolation broken)
- **Mitigation:** Add Organizations table to wipe scripts, ensure foreign keys handled correctly
- **Fallback:** Drop and recreate entire database between tests

### Implementation Steps

**Part 1: Update Database Wipe Scripts**

1. **Add Organizations table to database wipe logic**
   - Find database wipe scripts in E2E test setup
   - Add Organizations table to list of tables to truncate
   - Ensure foreign key constraints handled correctly (disable, truncate, re-enable)
   - Expected outcome: Database wipe includes Organizations
   - Files affected: `Test/e2e/E2eTests/Support/DatabaseHelper.cs` or similar
   - Reality check: Database wipe succeeds without FK errors

**Part 2: Create Multi-Tenancy Test Class**

2. **Create MultiTenancyHappyPathTests class**
   - Create `Test/e2e/E2eTests/MultiTenancy/MultiTenancyHappyPathTests.cs`
   - Inherit from AppPageTest (E2E base class)
   - Expected outcome: Test class structure ready
   - Files affected: `Test/e2e/E2eTests/MultiTenancy/MultiTenancyHappyPathTests.cs` (new)
   - Reality check: Test class compiles

**Part 3: Add Cross-Tenant Isolation Tests**

3. **Test: Users in different orgs cannot see each other's articles**
   - Register User1, create Article1
   - Register User2 (different org), list articles
   - Verify User2 does not see Article1
   - Expected outcome: Article isolation verified
   - Files affected: `Test/e2e/E2eTests/MultiTenancy/MultiTenancyHappyPathTests.cs`
   - Reality check: Test passes

4. **Test: Users in different orgs cannot see each other's comments**
   - User1 creates article with comment
   - User2 views articles (shouldn't see User1's article)
   - Verify User2 cannot see User1's comments
   - Expected outcome: Comment isolation verified
   - Files affected: `Test/e2e/E2eTests/MultiTenancy/MultiTenancyHappyPathTests.cs`
   - Reality check: Test passes

5. **Test: Users in same org can see each other's articles**
   - This requires admin functionality to add users to org (may be future feature)
   - For now, create manual test or skip (document as future test)
   - Expected outcome: Test created or documented as TODO
   - Files affected: `Test/e2e/E2eTests/MultiTenancy/MultiTenancyHappyPathTests.cs`
   - Reality check: Test or TODO documented

**Part 4: Add Tag and Profile Isolation Tests**

6. **Test: Tags are scoped to organization**
   - User1 creates article with tag "test"
   - User2 creates article with tag "test"
   - Verify each user only sees their own "test" tag (tags are tenant-scoped)
   - Expected outcome: Tag isolation verified
   - Files affected: `Test/e2e/E2eTests/MultiTenancy/MultiTenancyHappyPathTests.cs`
   - Reality check: Test passes

7. **Test: Follow relationships work within organization**
   - User1 in Org1 cannot follow User2 in Org2 (users can't see cross-org profiles)
   - Verify profile viewing is tenant-scoped
   - Expected outcome: Profile isolation verified
   - Files affected: `Test/e2e/E2eTests/MultiTenancy/MultiTenancyHappyPathTests.cs`
   - Reality check: Test passes

**Part 5: Run Tests**

8. **Run E2E tests**
   - Run: `./build.sh TestE2e`
   - Verify all new multi-tenancy tests pass
   - Verify existing E2E tests still pass
   - Expected outcome: All 51+ E2E tests pass
   - Reality check: E2E test suite green

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After database wipe updates
./build.sh TestE2e
# Should not fail with FK errors

# After each test added
./build.sh TestE2e

# Full validation
./build.sh TestE2e
```

Don't wait until the end to test. Reality test after each major change.

### Expected Working State After Phase

When this phase is complete:
- Database wipe scripts handle Organizations table correctly
- E2E tests verify cross-tenant isolation for articles, comments, tags
- E2E tests verify profile/follow relationships respect tenant boundaries
- Comprehensive E2E test coverage for multi-tenancy scenarios
- Regression prevention for tenant isolation bugs
- All E2E tests pass (51+ tests)
- Ready for functional test coverage in phases 11-12

### If Phase Fails

If this phase fails and cannot be completed:
1. Check database wipe scripts - ensure Organizations deleted without FK errors
2. Verify E2E test page objects use authentication correctly
3. Use Playwright trace viewer to debug test failures
4. Check that registration creates separate organizations for different users
5. Use debug-analysis.md for complex E2E issues
6. If stuck, run `flowpilot stuck`

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh TestE2e
```

All targets must pass before proceeding to the next phase.

**Manual Verification Steps:**
1. Review E2E test reports in `Reports/E2e/Artifacts/`
2. Verify new multi-tenancy tests are present
3. Check test execution logs for any warnings
4. Manually test cross-tenant isolation via UI if any E2E tests fail
