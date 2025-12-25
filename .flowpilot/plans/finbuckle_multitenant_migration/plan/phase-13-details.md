## phase_13: Add Functional Tests for Multi-Tenancy and Cross-Tenant Isolation

### Phase Overview

Add comprehensive functional tests at the handler/repository level to verify tenant isolation. Test that query filters work correctly for articles, comments, tags. Verify create operations set TenantId correctly. Ensure users cannot access other tenants' data via API.

**Scope Size:** Small (~8 steps)
**Risk Level:** Low (additive test coverage)
**Estimated Complexity:** Low

### Prerequisites

What must be completed before starting this phase:
- Phase 12 completed (slug uniqueness tenant-scoped)
- All tests passing
- System fully multi-tenant with comprehensive E2E coverage

### Known Risks & Mitigations

**Risk 1:** May duplicate E2E test coverage
- **Likelihood:** Medium
- **Impact:** Low (test redundancy)
- **Mitigation:** Focus functional tests on handler/repo level details that E2E tests don't cover
- **Fallback:** Accept some overlap for defense-in-depth testing

**Risk 2:** Test fixtures may need complex tenant setup
- **Likelihood:** Low (fixtures already use StaticStrategy from phase 8)
- **Impact:** Low (test complexity)
- **Mitigation:** Reuse existing fixture patterns from phase 8
- **Fallback:** Simplify tests to use single tenant if multi-tenant setup too complex

### Implementation Steps

**Part 1: Create Multi-Tenancy Functional Test Class**

1. **Create MultiTenancyIsolationTests class**
   - Create `App/Server/tests/Server.FunctionalTests/MultiTenancy/MultiTenancyIsolationTests.cs`
   - Inherit from appropriate test base class
   - Use existing fixture patterns for tenant setup
   - Expected outcome: Test class structure ready
   - Files affected: `App/Server/tests/Server.FunctionalTests/MultiTenancy/MultiTenancyIsolationTests.cs` (new)
   - Reality check: Test class compiles

**Part 2: Add Query Filter Tests**

2. **Test: Article queries filtered by tenant**
   - Create articles for Tenant1 and Tenant2
   - Set tenant context to Tenant1, query articles
   - Verify only Tenant1 articles returned
   - Expected outcome: Query filter test passes
   - Files affected: `App/Server/tests/Server.FunctionalTests/MultiTenancy/MultiTenancyIsolationTests.cs`
   - Reality check: Test passes

3. **Test: Comment queries filtered by tenant**
   - Create comments for articles in different tenants
   - Query comments with Tenant1 context
   - Verify only Tenant1 comments returned
   - Expected outcome: Comment filter test passes
   - Files affected: `App/Server/tests/Server.FunctionalTests/MultiTenancy/MultiTenancyIsolationTests.cs`
   - Reality check: Test passes

4. **Test: Tag queries filtered by tenant**
   - Create tags in different tenants
   - Query tags with Tenant1 context
   - Verify only Tenant1 tags returned
   - Expected outcome: Tag filter test passes
   - Files affected: `App/Server/tests/Server.FunctionalTests/MultiTenancy/MultiTenancyIsolationTests.cs`
   - Reality check: Test passes

**Part 3: Add Create Operation Tests**

5. **Test: Articles created with correct TenantId**
   - Set tenant context to Tenant1
   - Create article via CreateArticleHandler
   - Verify article.TenantId == Tenant1.Id
   - Expected outcome: TenantId assignment test passes
   - Files affected: `App/Server/tests/Server.FunctionalTests/MultiTenancy/MultiTenancyIsolationTests.cs`
   - Reality check: Test passes

6. **Test: Comments created with correct TenantId**
   - Set tenant context to Tenant1
   - Create comment via CreateCommentHandler
   - Verify comment.TenantId == Tenant1.Id
   - Expected outcome: Comment TenantId test passes
   - Files affected: `App/Server/tests/Server.FunctionalTests/MultiTenancy/MultiTenancyIsolationTests.cs`
   - Reality check: Test passes

**Part 4: Add Cross-Tenant Access Tests**

7. **Test: Users cannot access other tenants' articles via API**
   - Create article in Tenant1
   - Attempt to access article with Tenant2 authentication
   - Verify 404 Not Found or empty result (query filter blocks access)
   - Expected outcome: Cross-tenant access blocked
   - Files affected: `App/Server/tests/Server.FunctionalTests/MultiTenancy/MultiTenancyIsolationTests.cs`
   - Reality check: Test passes

**Part 5: Run Tests**

8. **Run functional tests**
   - Run: `./build.sh TestServer`
   - Verify all new multi-tenancy functional tests pass
   - Verify existing functional tests still pass
   - Expected outcome: All 45+ functional tests pass
   - Reality check: Functional test suite green

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After each test added
./build.sh TestServer

# Full validation (run individual Postman targets)
./build.sh TestServer
./build.sh TestServerPostmanArticlesEmpty
./build.sh TestServerPostmanAuth
./build.sh TestServerPostmanProfiles
./build.sh TestServerPostmanFeedAndArticles
./build.sh TestServerPostmanArticle
./build.sh TestE2e
```

Don't wait until the end to test. Reality test after each major change.

### Expected Working State After Phase

When this phase is complete:
- Functional tests verify tenant isolation at handler/repository level
- Tests validate query filters work correctly for articles, comments, tags
- Tests verify create operations set TenantId correctly
- Tests ensure users cannot access other tenants' data
- Comprehensive functional test coverage for multi-tenancy
- All functional tests pass (45+ tests)
- Complete test coverage (E2E + functional) for multi-tenant scenarios
- Ready for final logging enhancements in phase 13

### If Phase Fails

If this phase fails and cannot be completed:
1. Review existing functional test patterns for authentication and tenant setup
2. Check that StaticStrategy is configured correctly in test fixtures
3. Use debug-analysis.md for complex test issues
4. If stuck, run `flowpilot stuck`

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh TestServer
./build.sh TestServerPostmanArticlesEmpty
./build.sh TestServerPostmanAuth
./build.sh TestServerPostmanProfiles
./build.sh TestServerPostmanFeedAndArticles
./build.sh TestServerPostmanArticle
./build.sh TestE2e
```

All targets must pass before proceeding to the next phase.

**Manual Verification Steps:**
1. Review functional test reports in `Reports/Server/Artifacts/Tests/`
2. Verify new multi-tenancy tests are present and passing
3. Check test coverage metrics (if available)
4. Verify no test flakiness in multi-tenancy tests
