## phase_5: Add TenantId to Domain Entities and Enable Query Filters

### Phase Overview

Mark EntityBase with [MultiTenant] attribute so all entities that inherit from it (Article, Tag, Comment) automatically become tenant-scoped. Add TenantId property to EntityBase with EF configuration and index. Global query filters will automatically apply to restrict data by TenantId.

**Scope Size:** Small (~8 steps)
**Risk Level:** Low (additive changes, filters are automatic)
**Estimated Complexity:** Low

### Prerequisites

What must be completed before starting this phase:
- Phase 4 completed (Organization entity and AppDbContext updated)
- All tests passing with Organization creation in fixtures
- Database has Organizations table and ApplicationUser.TenantId

### Known Risks & Mitigations

**Risk 1:** Query filters may cause unexpected empty results
- **Likelihood:** High
- **Impact:** Medium (tests may fail, queries return no data)
- **Mitigation:** Entities created before this phase won't have TenantId set, so queries may return empty. Tests will need updates to set TenantId on created entities or use IgnoreQueryFilters() temporarily.
- **Fallback:** Use IgnoreQueryFilters() in specifications if needed for transition

**Risk 2:** Specifications may need updates for tenant-aware queries
- **Likelihood:** Medium
- **Impact:** Medium (some queries may bypass filters unintentionally)
- **Mitigation:** Review all specifications in Server.Core/Specifications to ensure they don't conflict with query filters
- **Fallback:** Add explicit TenantId checks to specifications if filters don't apply

### Implementation Steps

**Part 1: Update EntityBase**

1. **Add TenantId property to EntityBase**
   - Open `App/Server/src/Server.Core/Entities/EntityBase.cs`
   - Add property: `public Guid? TenantId { get; set; }`
   - Add [MultiTenant] attribute to the class
   - Expected outcome: All entities inheriting from EntityBase get TenantId
   - Files affected: `App/Server/src/Server.Core/Entities/EntityBase.cs`
   - Reality check: Article, Tag, Comment now have TenantId property automatically

2. **Configure TenantId in EntityBase configuration**
   - Open or create `EntityBaseConfiguration.cs` in `App/Server/src/Server.Infrastructure/Data/Config/`
   - Add configuration: `builder.Property<Guid?>("TenantId");` (shadow property config)
   - Or configure in AppDbContext OnModelCreating: `modelBuilder.Entity<EntityBase>().HasIndex(e => e.TenantId);`
   - Add index on TenantId for performance: `builder.HasIndex(e => e.TenantId);`
   - Expected outcome: TenantId configured with index
   - Files affected: `App/Server/src/Server.Infrastructure/Data/Config/EntityBaseConfiguration.cs` or `AppDbContext.cs`
   - Reality check: Build succeeds

**Part 2: Create and Apply EF Migration**

3. **Create EF Core migration and regenerate idempotent script**
   - Run: `dotnet ef migrations add AddTenantIdToEntities --project App/Server/src/Server.Infrastructure --startup-project App/Server/src/Server.Web`
   - Regenerate idempotent script: `dotnet ef migrations script --idempotent --output App/Server/src/Server.Infrastructure/Data/Migrations/idempotent.sql --project App/Server/src/Server.Infrastructure --startup-project App/Server/src/Server.Web`
   - Review migration file - verify TenantId column added to Articles, Tags, Comments tables
   - Verify indexes created on TenantId columns
   - Expected outcome: Migration file and idempotent script created
   - Files affected: `App/Server/src/Server.Infrastructure/Data/Migrations/*_AddTenantIdToEntities.cs` (new), `App/Server/src/Server.Infrastructure/Data/Migrations/idempotent.sql` (updated)
   - Reality check: Migration file shows TenantId columns for Article, Tag, Comment, run `./build.sh DbMigrationsVerifyIdempotentScript` and `./build.sh DbMigrationsVerifyAll` to verify

4. **Verify migrations**
   - Migrations run automatically on app startup
   - Run: `./build.sh DbMigrationsVerifyAll` to verify migrations apply successfully
   - Verify Articles, Tags, Comments tables have TenantId column
   - Verify indexes exist
   - Expected outcome: Database schema updated, all migration verifications pass
   - Reality check: DbMigrationsVerifyAll target passes

**Part 3: Test Query Filters**

5. **Verify query filters work**
   - Use RoslynMCP FindUsages to find handlers that query Article, Tag, Comment entities
   - Run individual Postman collection targets: `./build.sh TestServerPostmanArticlesEmpty`, `./build.sh TestServerPostmanAuth`, etc.
   - Inspect generated SQL in logs at `Logs/Test/Postman/Server.Web/Serilog/` after running tests
   - Verify queries include `WHERE TenantId = [tenant]` clause (check if EF Core SQL logging is already enabled)
   - Expected outcome: Query filters automatically apply
   - Reality check: SQL logs show WHERE TenantId clause

6. **Update functional test fixtures**
   - Modify test fixtures to set TenantId on created entities
   - Or use StaticStrategy to provide tenant context (will be added in phase 8)
   - For now, may need to use `IgnoreQueryFilters()` in test specs temporarily
   - Expected outcome: Tests can query entities successfully
   - Files affected: `App/Server/tests/Server.FunctionalTests/Fixtures/*.cs`
   - Reality check: Test setup compiles

**Part 4: Run Tests**

7. **Run functional tests**
   - Run: `./build.sh TestServer`
   - Fix tests that fail due to query filters returning empty results
   - Entities without TenantId won't be returned by filtered queries
   - Expected outcome: Tests pass (may need adjustments)
   - Reality check: `./build.sh TestServer` succeeds

8. **Run all tests**
   - Run individual Postman collection targets:
     - `./build.sh TestServerPostmanArticlesEmpty`
     - `./build.sh TestServerPostmanAuth`
     - `./build.sh TestServerPostmanProfiles`
     - `./build.sh TestServerPostmanFeedAndArticles`
     - `./build.sh TestServerPostmanArticle`
   - Run: `./build.sh TestE2e`
   - Expected outcome: All tests pass
   - Reality check: CI-level confidence

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After EntityBase changes
./build.sh LintServerVerify
./build.sh BuildServer

# After migration and idempotent script
./build.sh DbMigrationsVerifyIdempotentScript
./build.sh DbMigrationsVerifyAll
# Verify migrations apply successfully

# After test updates
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
- EntityBase has TenantId property and [MultiTenant] attribute
- Article, Tag, Comment automatically have TenantId (inherit from EntityBase)
- Database has TenantId columns on Articles, Tags, Comments tables
- Indexes created on TenantId columns for performance
- Global query filters automatically apply (WHERE TenantId = [tenant])
- Tests pass (may use IgnoreQueryFilters() temporarily or set TenantId explicitly)
- **Query filters are active but no tenant resolution yet** (phase 8 adds tenant resolution)
- Entities created without TenantId won't be returned by queries (expected)

### If Phase Fails

If this phase fails and cannot be completed:
1. If query filters cause too many test failures, add IgnoreQueryFilters() to specifications temporarily
2. Use RoslynMCP FindUsages to find all usages of IRepository<Article>, IRepository<Tag>, IRepository<Comment>
3. Check EF Core logs to verify query filters are applied correctly
4. If entities don't have TenantId, queries will return empty - this is expected until phase 8-9
5. Use debug-analysis.md for complex query issues
6. If stuck, run `flowpilot stuck`

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh DbMigrationsVerifyAll
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
1. Inspect database schema - verify TenantId columns exist on Articles, Tags, Comments
2. Check indexes on TenantId columns
3. Enable EF Core SQL logging and verify queries include WHERE TenantId clause
4. Check that queries return empty results when TenantId is not set (expected behavior)
5. Start application and verify no errors
