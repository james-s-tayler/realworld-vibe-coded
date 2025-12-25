## phase_12: Update Slug Uniqueness Validation for Tenant Scope

### Phase Overview

Update article slug uniqueness validation from global uniqueness to tenant-scoped uniqueness. Articles in different organizations can now have the same slug. Update validation logic and tests to reflect multi-tenant slug semantics.

**Scope Size:** Small (~5 steps)
**Risk Level:** Low (validation logic update)
**Estimated Complexity:** Low

### Prerequisites

What must be completed before starting this phase:
- Phase 11 completed (E2E multi-tenancy tests passing)
- All tests passing
- Understanding of current slug validation logic

### Known Risks & Mitigations

**Risk 1:** Slug validation may be in multiple places
- **Likelihood:** Low
- **Impact:** Low (need to update all locations)
- **Mitigation:** Use RoslynMCP FindUsages to find all slug validation code
- **Fallback:** Grep for "slug" validation logic

**Risk 2:** Unique index on slug may conflict
- **Likelihood:** Low (probably no global unique index)
- **Impact:** Medium (database constraint violation)
- **Mitigation:** Review database schema, remove global unique index if exists
- **Fallback:** Create composite unique index on (TenantId, Slug)

### Implementation Steps

**Part 1: Review Current Slug Validation**

1. **Find slug validation logic**
   - Use RoslynMCP FindUsages or grep to find slug uniqueness checks
   - Look in CreateArticleHandler, UpdateArticleHandler, validators
   - Identify if validation queries include TenantId filter
   - Expected outcome: All slug validation locations identified
   - Files affected: TBD (find first)
   - Reality check: Know where slug validation happens

**Part 2: Update Slug Validation Logic**

2. **Update slug uniqueness validation**
   - Modify slug validation to check uniqueness within current tenant only
   - Query: `Articles.Where(a => a.Slug == slug && a.TenantId == currentTenantId)`
   - Query filters should handle TenantId automatically, but make explicit if needed
   - Expected outcome: Slug validation is tenant-scoped
   - Files affected: `App/Server/src/Server.Web/Endpoints/Articles/Validators/*` or handler
   - Reality check: Validation checks tenant-scoped uniqueness

**Part 3: Update Database Schema (If Needed)**

3. **Check for global unique index on slug**
   - Inspect database schema for unique index on Articles.Slug
   - If global unique index exists, create migration to remove it
   - Create composite unique index on (TenantId, Slug) instead
   - Expected outcome: Schema allows duplicate slugs across tenants
   - Files affected: New EF migration if needed
   - Reality check: Database schema correct

**Part 4: Update Tests**

4. **Add test: Same slug in different organizations**
   - User1 creates article with slug "hello-world"
   - User2 (different org) creates article with slug "hello-world"
   - Both succeed - slug uniqueness is per-tenant
   - Expected outcome: Test validates tenant-scoped uniqueness
   - Files affected: `App/Server/tests/Server.FunctionalTests/Articles/ArticleSlugTests.cs` (new or existing)
   - Reality check: Test passes

5. **Update existing slug uniqueness tests**
   - Ensure existing tests create articles within same tenant context
   - Tests should fail if duplicate slug in same tenant
   - Expected outcome: Existing slug validation tests still valid
   - Files affected: Existing slug tests
   - Reality check: Tests pass

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After validation updates
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer

# Full validation (run individual Postman targets)
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
- Article slug uniqueness is tenant-scoped (not global)
- Articles in different organizations can have same slug
- Validation logic checks uniqueness within current tenant
- Database schema allows duplicate slugs across tenants
- Tests verify tenant-scoped slug uniqueness
- All tests pass

### If Phase Fails

If this phase fails and cannot be completed:
1. Use RoslynMCP FindUsages to find all slug validation code
2. Check database schema for unique constraints
3. Use debug-analysis.md for complex validation issues
4. If stuck, run `flowpilot stuck`

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
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
1. Start application: `./build.sh RunLocalPublish`
2. Register User1, create article with slug "test-article"
3. Register User2 (different org), create article with slug "test-article"
4. Verify both articles created successfully
5. Verify User1 cannot create another article with slug "test-article" (within same org)
