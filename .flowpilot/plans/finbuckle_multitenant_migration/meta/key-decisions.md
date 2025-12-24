## Key Decision Points

Before proceeding to phase analysis, critical architectural and strategic decisions must be made. Each decision should be informed by research and system analysis.

## Decision-Making Guidelines

For each decision:
1. **Research First** - Ensure references.md has relevant information
2. **Consider System Context** - Reference specific findings from system-analysis.md
3. **Evaluate Trade-offs** - List pros/cons for each option
4. **Test Assumptions** - For critical decisions, create proof-of-concept to validate approach
5. **Document Rationale** - Explain why the chosen option is best for this specific system

## Critical Decision Checklist

Ensure key decisions cover:
- [x] **Data Strategy** - Preserve existing data or clean slate?
- [x] **Backward Compatibility** - Maintain API compatibility or allow breaking changes?
- [x] **Migration Approach** - Big bang, incremental, or parallel run?
- [x] **Testing Strategy** - Update tests incrementally or all at once?
- [x] **Rollback Strategy** - How to safely rollback if migration fails?
- [x] **Performance** - Any performance implications identified and mitigated?

---

## Decision 1: DbContext Inheritance Strategy (Audit.NET + Finbuckle Integration)

### Context

**Current state:** 
- AppDbContext inherits from `AuditIdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` (from Audit.EntityFramework.Identity.Core v31.3.1)
- AuditIdentityDbContext provides automatic EntityFramework auditing for all changes
- All EF operations are logged via Audit.NET with transaction tracking

**Problem to solve:**
- Need to integrate Finbuckle.MultiTenant which requires deriving from `MultiTenantIdentityDbContext` (from Finbuckle.MultiTenant.EntityFrameworkCore v10.0.0)
- Both AuditIdentityDbContext and MultiTenantIdentityDbContext derive from IdentityDbContext<TUser, TRole, TKey>
- Cannot inherit from both classes (C# single inheritance constraint)
- Must maintain audit logging capabilities while adding multi-tenancy support

**Constraints:**
- Must preserve Audit.NET functionality (required for compliance)
- Must integrate Finbuckle's automatic query filtering and tenant association
- Cannot use multiple inheritance in C#
- Must support both Cookie and Bearer token authentication schemes

**Related systems:**
- Affects all 19 MediatR handlers that use IRepository
- Affects all 45 functional tests that use WebApplicationFactory
- Affects 51 E2E tests that rely on database state
- Affects Audit.NET log analysis workflows

### Options

### Option A: Custom DbContext Implementing IMultiTenantDbContext with Manual Audit Integration

**Description:** 
Create AppDbContext that inherits from `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` and manually implements both:
1. `IMultiTenantDbContext` interface from Finbuckle (for multi-tenancy)
2. Manual Audit.NET configuration (for auditing)

This approach replicates the behavior of both parent classes through explicit implementation.

**Pros:**
- Complete control over both auditing and multi-tenancy behaviors
- Can customize exactly what gets audited and how tenancy is enforced
- No hidden base class behaviors that could conflict
- Clear separation of concerns between tenancy and auditing logic

**Cons:**
- Must manually implement `IMultiTenantDbContext` interface methods (SaveChangesAsync, OnModelCreating logic)
- Must manually configure Audit.NET instead of inheriting AuditIdentityDbContext
- More code to write and maintain (approximately 150-200 lines of boilerplate)
- Risk of missing implementation details from either base class
- Requires deeper understanding of both libraries' internal implementations

**Impact:**
- Code changes: ~200 lines in AppDbContext.cs, manual OnModelCreating configuration
- Database changes: None (same schema as other options)
- Test changes: None (tests use AppDbContext via interface)
- Risk level: **HIGH** - Manual implementation may miss critical Finbuckle or Audit.NET behaviors
- Reversibility: **Medium** - Can revert to base class inheritance if issues found, but requires significant rework

**Supporting Research:**
- Finbuckle docs show IMultiTenantDbContext interface as alternative to base class inheritance (references.md section on EF Core Integration)
- Audit.NET has manual configuration options documented (though less common than using base class)
- No community examples found combining both approaches manually

### Option B: Use MultiTenantIdentityDbContext and Configure Audit.NET via Data Provider

**Description:**
Inherit AppDbContext from `MultiTenantIdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` (from Finbuckle) and configure Audit.NET to audit EF operations using the Audit.EntityFramework data provider directly, without using AuditIdentityDbContext base class.

This leverages Finbuckle's base class while adding Audit.NET as an orthogonal concern via data provider configuration.

**Pros:**
- Leverages Finbuckle's well-tested base class for multi-tenancy (query filters, tenant association, unique indexes)
- Audit.NET data provider approach is officially supported and documented
- All Identity entities automatically configured as multi-tenant (v10+ behavior)
- Simpler OnModelCreating - just call base implementation
- Lower risk of missing Finbuckle behaviors (all included in base class)

**Cons:**
- Loses some automatic audit configuration from AuditIdentityDbContext
- Must manually configure Audit.EntityFramework data provider in SaveChangesAsync
- Need to ensure TenantId is included in audit events manually
- Slightly more configuration code for Audit.NET setup

**Impact:**
- Code changes: ~50-75 lines to configure Audit.NET data provider in AppDbContext
- Database changes: None (same audit tables)
- Test changes: None (tests use AppDbContext via interface)
- Risk level: **MEDIUM** - Audit.NET configuration may need tuning, but both libraries are independently proven
- Reversibility: **Easy** - Can switch inheritance back to AuditIdentityDbContext if multi-tenancy removed

**Supporting Research:**
- Finbuckle docs explicitly recommend deriving from MultiTenantIdentityDbContext for Identity integration (references.md)
- Audit.NET documentation covers data provider approach as primary configuration method
- Community examples show successful Audit.NET usage without base class inheritance

### Option C: Use AuditIdentityDbContext and Manually Implement Multi-Tenancy Features

**Description:**
Keep AppDbContext inheriting from `AuditIdentityDbContext` and manually add Finbuckle multi-tenancy features:
- Manual query filters for TenantId
- Manual tenant association in SaveChangesAsync
- Manual unique index configuration including TenantId

**Pros:**
- Preserves existing Audit.NET base class and all its automatic behaviors
- No changes to audit logging infrastructure
- Familiar inheritance chain for existing developers

**Cons:**
- Must manually replicate all Finbuckle.MultiTenant functionality (query filters, tenant association, validation)
- Risk of security issues if manual implementation has gaps
- Misses out on Finbuckle's well-tested edge case handling
- Identity entities won't be automatically configured as multi-tenant
- Must manually handle unique index configuration with TenantId
- More complex OnModelCreating logic
- Defeats the purpose of using Finbuckle library

**Impact:**
- Code changes: ~300+ lines to replicate Finbuckle behaviors manually
- Database changes: Same as other options (TenantId columns, indexes)
- Test changes: Need extensive tests to verify manual multi-tenancy implementation
- Risk level: **VERY HIGH** - Reimplementing security-critical library functionality manually
- Reversibility: **Easy** - Already using AuditIdentityDbContext, just remove manual code

**Supporting Research:**
- References.md explicitly recommends against custom multi-tenancy implementations (Alternative Approaches section)
- System analysis identifies that manual implementation would require ~300+ lines and has high security risk

### Proof of Concept Required?

- [x] Yes - Create POC to validate approach before committing
- [ ] No - Decision can be made based on research and analysis

**POC Scope:** 
Create minimal POC with Option B to verify:
1. AppDbContext successfully inherits from MultiTenantIdentityDbContext
2. Audit.NET data provider can be configured to audit EF operations
3. Both multi-tenancy (query filters) and auditing (event capture) work together
4. TenantId is captured in audit events
5. SaveChangesAsync successfully coordinates both concerns

POC should include:
- AppDbContext with MultiTenantIdentityDbContext inheritance
- Audit.NET data provider configuration
- Simple entity (Article) with TenantId
- Test that inserts entity and verifies both query filtering and audit log capture

### Choice

- [x] Option B: Use MultiTenantIdentityDbContext and Configure Audit.NET via Data Provider
- [ ] Option A: Custom DbContext Implementing IMultiTenantDbContext with Manual Audit Integration
- [ ] Option C: Use AuditIdentityDbContext and Manually Implement Multi-Tenancy Features

**Rationale:**

1. **Best fit for this system because:** Our system-analysis.md identifies that we have 19 handlers, 45 functional tests, and 51 E2E tests that depend on AppDbContext. Using Finbuckle's base class minimizes risk of breaking existing functionality while adding multi-tenancy. The automatic query filters and tenant association from MultiTenantIdentityDbContext are battle-tested and handle edge cases we'd likely miss in manual implementation.

2. **Addresses key constraints:** Preserves both audit logging (via data provider) and gains multi-tenancy (via base class). Meets compliance requirements for auditing while adding required tenant isolation.

3. **Mitigates identified risks:** References.md documents known issues with custom multi-tenancy implementations and recommends using Finbuckle's base class. This option follows documented best practices from both libraries. The POC will validate that Audit.NET data provider works correctly with Finbuckle inheritance.

4. **Aligns with research:** References.md section "Finbuckle.MultiTenant - Identity Integration" explicitly states: "Derive from MultiTenantIdentityDbContext (which itself derives from IdentityDbContext) for automatic multi-tenant configuration." Audit.NET documentation shows data provider as primary configuration approach.

5. **Practical considerations:** Option A requires ~200 lines of manual implementation with high risk. Option C requires ~300+ lines and defeats the purpose of using Finbuckle. Option B requires only ~50-75 lines of Audit.NET configuration, leverages both libraries' strengths, and has medium risk that can be validated via POC.

### Implementation Notes

Critical guidance for implementing this decision:

- **SaveChangesAsync override:** Must coordinate Finbuckle's tenant association (from base.SaveChangesAsync) with Audit.NET event capture. Call base.SaveChangesAsync and configure Audit.NET to capture the same transaction.

- **TenantId in audit events:** Configure Audit.NET to include TenantId (from IMultiTenantContextAccessor) as custom field in all audit events. This ensures audit logs can be filtered by tenant.

- **Transaction semantics:** Verify that Audit.NET events are part of the same database transaction as the entity changes. Audit.NET should use the same DbContext transaction.

- **Test isolation:** Update functional tests to use Finbuckle's StaticStrategy for tenant resolution. Each test should set a known tenant ID to avoid cross-test contamination.

### Validation Criteria

How to confirm this decision was correct:

- **POC successful:** POC demonstrates that AppDbContext with MultiTenantIdentityDbContext inheritance and Audit.NET data provider successfully captures audited events with TenantId and enforces query filters. All three components (EF Core, Finbuckle, Audit.NET) work together without conflicts.

- **Tests pass:** All 45 existing functional tests pass after migrating to MultiTenantIdentityDbContext (with StaticStrategy in test setup). No regression in existing functionality.

- **Audit logs contain tenant context:** Manual inspection of Audit.NET logs confirms that TenantId is present in all audited events after implementation. Logs can be filtered by tenant.

---

## Decision 2: Data Migration Strategy

### Context

**Current state:**
- Database has existing schema: AspNetUsers, Articles, Comments, Tags, UserFollowings, etc.
- No production data (pre-production application)
- Clean Sqlite database used for development

**Problem to solve:**
- Need to add Organization table and TenantId columns to multiple tables
- Need to establish foreign key relationships between entities and Organizations
- Determine if existing data should be preserved or if clean slate is acceptable

**Constraints:**
- Requirements explicitly state: "Do not worry about needing to migrate or maintain compatibility with existing data. This is a pre-production application with no users."
- Development team may have test data in local databases
- CI/CD pipeline uses fresh database for each test run

**Related systems:**
- Affects all 45 functional tests that may have test data setup
- Affects 51 E2E tests that create test data via UI
- Affects Postman collections with pre-seeded test data

### Options

### Option A: Clean Slate - Drop and Recreate Database

**Description:**
Delete existing database and recreate from scratch with new multi-tenant schema. All migrations created as if starting fresh.

**Pros:**
- Simplest approach - no data migration logic needed
- No risk of data corruption during migration
- Schema is clean with no legacy structures
- Can optimize schema design without backward compatibility constraints
- Matches requirements explicitly ("do not worry about existing data")

**Cons:**
- Developers lose any local test data
- Must recreate test data after migration
- Cannot rollback to preserve specific test scenarios

**Impact:**
- Code changes: None (just schema changes in new migrations)
- Database changes: Complete recreation with Organization table and TenantId columns
- Test changes: Test fixtures need to create Organizations before users
- Risk level: **LOW** - Requirements explicitly allow this
- Reversibility: **HARD** - Cannot recover dropped data, but requirements don't require preservation

**Supporting Research:**
- Goal.md explicitly states: "Do not worry about needing to migrate or maintain compatibility with existing data. This is a pre-production application with no users."
- System-analysis.md confirms this simplification: "Clean Slate for Data: Requirements state this is pre-production with no users, so no data migration needed. This is a massive simplification that allows us to make breaking schema changes freely."

### Option B: Preserve Data with Default Organization

**Description:**
Keep existing data and migrate it to a default "Legacy" organization. Add TenantId columns with FK constraints and update all existing rows.

**Pros:**
- Preserves any existing test data
- Demonstrates data migration patterns for future use
- Can validate migration logic with real data

**Cons:**
- More complex - requires data migration scripts
- Must handle foreign key constraints during migration
- Risk of migration failures with data integrity issues
- Doesn't match requirements (explicitly says data preservation not needed)
- Slower migration execution
- Must create artificial "Legacy" organization that doesn't represent real use case

**Impact:**
- Code changes: ~100-150 lines of migration logic to create default org and update FKs
- Database changes: Same schema as Option A, but with data migration scripts
- Test changes: Same as Option A, but must account for legacy organization
- Risk level: **MEDIUM** - Data migration can fail in unexpected ways
- Reversibility: **MEDIUM** - Can rollback migration, but complexity increases risk

**Supporting Research:**
- Common pattern in production systems, but not applicable per requirements
- System-analysis.md identifies this as unnecessary given pre-production status

### Proof of Concept Required?

- [ ] Yes - Create POC to validate approach before committing
- [x] No - Decision can be made based on research and analysis

**POC Scope:** N/A - Requirements are explicit about this decision.

### Choice

- [x] Option A: Clean Slate - Drop and Recreate Database
- [ ] Option B: Preserve Data with Default Organization

**Rationale:**

1. **Best fit for this system because:** Goal.md explicitly states we should "not worry about needing to migrate or maintain compatibility with existing data." The system is pre-production with no users, so data preservation adds complexity without benefit.

2. **Addresses key constraints:** Matches the explicit requirement to not preserve data. Simplifies migration by eliminating data migration logic, reducing risk and development time.

3. **Mitigates identified risks:** System-analysis.md identifies data migration as potential high-risk item. By choosing clean slate, we eliminate this risk entirely. No risk of data corruption, foreign key violations, or migration script failures.

4. **Aligns with research:** System-analysis.md observation #10 states: "Clean Slate for Data: Requirements state this is pre-production with no users, so no data migration needed. This is a massive simplification that allows us to make breaking schema changes freely."

5. **Practical considerations:** Clean slate allows us to optimize schema design without backward compatibility concerns. Can add constraints, indexes, and relationships that would be difficult to retrofit onto existing data. Significantly reduces migration complexity and testing burden.

### Implementation Notes

Critical guidance for implementing this decision:

- **Communication:** Notify developers that local databases will need to be dropped and recreated. Provide clear instructions in migration documentation.

- **Migration approach:** Create new migrations from scratch or use `dotnet ef migrations remove` to clean out old migrations, then create fresh initial migration with Organization table and TenantId columns already included.

- **Test fixtures:** Update all test fixtures to create Organizations before creating users. E2E tests' database wipe scripts must include Organizations table.

- **Development workflow:** Update README or development setup docs to indicate that `dotnet ef database drop` and `dotnet ef database update` should be run after pulling changes.

### Validation Criteria

How to confirm this decision was correct:

- **Clean migration execution:** Fresh database can be created from migrations without errors. `dotnet ef database update` completes successfully.

- **Tests pass with fresh data:** All functional tests (45), E2E tests (51), and Postman collections (5) pass with fresh database and test data creation logic.

- **No legacy data references:** Code has no references to handling missing TenantId or default organizations. All entities assume TenantId is always present.

---

## Decision 3: Migration Approach (Incremental Phases vs Big Bang)

### Context

**Current state:**
- Working application with 19 MediatR handlers, 45 functional tests, 51 E2E tests, 5 Postman collections
- All tests passing in CI/CD pipeline
- Clean Architecture with clear layer boundaries

**Problem to solve:**
- Need to add multi-tenancy across all layers: domain entities, database schema, handlers, endpoints, authentication, tests
- Determine whether to make all changes at once or in incremental phases
- Balance risk of breaking existing functionality vs complexity of managing partial migration state

**Constraints:**
- Must maintain working state at end of each PR/phase (CI/CD must stay green)
- Each phase must be reviewable and testable independently
- Cannot break existing API contracts without coordination with frontend
- Must keep team productive during migration

**Related systems:**
- 19 handlers depend on AppDbContext and repository queries
- 45 functional tests assume specific database schema and data setup
- 51 E2E tests assume specific registration and authentication flows
- React SPA makes API calls expecting certain response formats

### Options

### Option A: Incremental Phases with Working State Between Each

**Description:**
Break migration into 5-7 small phases, each leaving the codebase in a working, testable state. Each phase focuses on one aspect (e.g., infrastructure, then domain, then handlers, then tests).

**Pros:**
- Lower risk - each phase is independently testable and reviewable
- Easier to rollback if issues found
- Can validate approach incrementally before committing to full migration
- Smaller PRs are easier to review
- Team can continue development on other features between phases
- Matches phase-based approach recommended by FlowPilot workflow

**Cons:**
- More total commits and PR overhead
- Potential for temporary "scaffolding" code that exists only to keep tests passing mid-migration
- Must carefully plan phase boundaries to ensure working state
- Longer calendar time to complete migration (though less total dev time)

**Impact:**
- Code changes: Spread across 5-7 phases, ~50-100 lines per phase
- Database changes: Incremental migrations, one per phase
- Test changes: Incremental - update tests as affected by each phase
- Risk level: **LOW** - Each phase is small and independently validated
- Reversibility: **EASY** - Can rollback individual phases without affecting others

**Supporting Research:**
- System-analysis.md notes: "Test Maintenance is Significant: With 45 functional tests, 51 E2E tests, and 5 Postman collections, test maintenance will be a major effort in every phase. Each phase must explicitly account for test updates or risk breaking CI/CD."
- References.md on incremental migration patterns shows community preference for phased approach in complex systems
- FlowPilot workflow is explicitly designed for incremental, phase-based migrations

### Option B: Big Bang - All Changes in One Phase

**Description:**
Make all changes in a single large PR: add Organization entity, update all domain entities with TenantId, migrate AppDbContext, update all handlers, update all tests, update authentication.

**Pros:**
- Fastest calendar time to completion
- No intermediate states to maintain
- No scaffolding code needed
- Single large PR can be reviewed holistically
- Simpler to understand "before" and "after" states

**Cons:**
- Very high risk - if anything breaks, entire migration must be rolled back
- Large PR is difficult to review thoroughly (500+ lines)
- Hard to identify root cause if tests fail
- Cannot validate approach incrementally
- Blocks other development work while in progress
- If rollback needed, loses all work from phase
- Does not follow FlowPilot workflow design

**Impact:**
- Code changes: Single PR with 500-800 lines changed across all layers
- Database changes: Single large migration with all schema changes
- Test changes: All 45+51+5 test suites updated at once
- Risk level: **VERY HIGH** - All-or-nothing approach with no incremental validation
- Reversibility: **HARD** - Must revert entire PR, losing all work

**Supporting Research:**
- System-analysis.md observation #2: "Test Maintenance is Significant: With 45 functional tests, 51 E2E tests, and 5 Postman collections, test maintenance will be a major effort in every phase."
- References.md shows no community examples of successful big-bang multi-tenancy migrations for systems of this complexity
- FlowPilot documentation explicitly recommends against big-bang migrations

### Proof of Concept Required?

- [ ] Yes - Create POC to validate approach before committing
- [x] No - Decision can be made based on research and analysis

**POC Scope:** N/A - The incremental approach is lower risk regardless of system specifics.

### Choice

- [x] Option A: Incremental Phases with Working State Between Each
- [ ] Option B: Big Bang - All Changes in One Phase

**Rationale:**

1. **Best fit for this system because:** System-analysis.md identifies significant test infrastructure (45 functional + 51 E2E + 5 Postman = 101 test suites) that must be maintained throughout migration. Incremental phases allow updating tests as each area is affected, rather than all at once. Clean Architecture boundaries make it natural to migrate one layer at a time.

2. **Addresses key constraints:** Must maintain working CI/CD state between PRs. Incremental phases ensure CI stays green throughout migration. Each phase is independently reviewable and testable, making it easier to catch issues early.

3. **Mitigates identified risks:** References.md documents known issues with multi-tenancy migrations, including authentication flows, query filtering, and test isolation. Incremental approach allows validating each concern independently. If POC from Decision 1 fails, only that phase needs rework, not entire migration.

4. **Aligns with research:** FlowPilot workflow is explicitly designed for incremental, phase-based migrations with working state between each phase. System-analysis.md repeatedly emphasizes test maintenance burden, which favors incremental updates.

5. **Practical considerations:** Smaller PRs are easier to review and less likely to introduce subtle bugs. Team can continue other work between phases without merge conflicts. Calendar time is longer but total dev effort is lower (no debugging mega-PR that touches everything).

### Implementation Notes

Critical guidance for implementing this decision:

- **Phase boundaries:** Plan phases around Clean Architecture layer boundaries and test infrastructure updates. Example: (1) Infrastructure/DbContext, (2) Domain entities, (3) Handlers, (4) Authentication, (5) Functional tests, (6) E2E tests.

- **Working state definition:** Each phase must pass all existing tests (even if not all functionality is migrated). Use feature flags or parallel implementations if needed to keep old code paths working until fully migrated.

- **Test strategy per phase:** Update test fixtures incrementally as each phase affects them. Don't let tests break mid-migration - update them in same phase that changes underlying behavior.

- **Migration order:** Start with infrastructure (POC validation), then domain layer, then application layer (handlers), then tests. This matches dependency direction and minimizes rework.

### Validation Criteria

How to confirm this decision was correct:

- **CI stays green:** After each phase, all existing tests pass. CI/CD pipeline completes successfully for each PR.

- **Independent reviewability:** Each PR is small enough (< 300 lines) to be thoroughly reviewed. Reviewers can understand changes without reviewing entire migration context.

- **Rollback feasibility:** If any phase needs rollback, it can be reverted without affecting other completed phases. Code remains in working state after rollback.

---

## Decision 4: Testing Strategy (Incremental vs All at Once)

### Context

**Current state:**
- 45 functional tests using WebApplicationFactory and FastEndpoints.Testing
- 51 Playwright E2E tests through React SPA
- 5 Postman/Newman collections with comprehensive API coverage
- Tests assume single-tenant (no organization context)

**Problem to solve:**
- All tests need tenant context to work with multi-tenant system
- Determine whether to update tests incrementally (as each phase affects them) or all at once (in dedicated test migration phase)
- Balance test maintenance overhead vs keeping CI green

**Constraints:**
- Must maintain CI/CD green status throughout migration
- Tests are comprehensive and provide confidence in system correctness
- Functional tests use fixtures that setup test users and data
- E2E tests wipe database between runs using scripts
- Postman collections test actual API contracts used by frontend

**Related systems:**
- Test fixtures (ArticlesFixture, ProfilesFixture, etc.) used by functional tests
- Database wipe scripts used by E2E tests
- Postman environment variables and pre-request scripts

### Options

### Option A: Incremental Test Updates (Update Tests with Each Phase)

**Description:**
Update tests incrementally as each migration phase affects them. When adding Organization entity in Phase 1, update test fixtures to create Organizations. When adding TenantId to Articles in Phase 2, update Article-related tests. Continue pattern through all phases.

**Pros:**
- Tests stay synchronized with implementation changes
- Validates each phase's functionality immediately
- Lower risk of "test debt" accumulating
- Easier to identify which changes broke which tests
- Matches incremental migration approach from Decision 3

**Cons:**
- Test updates are spread across multiple phases
- May need temporary scaffolding in tests to support partial migration
- Each phase has both implementation and test update overhead
- Could feel repetitive as test infrastructure is touched multiple times

**Impact:**
- Code changes: Test updates distributed across 5-7 phases
- Test changes: ~10-15 tests updated per phase as affected
- Risk level: **LOW** - Tests remain aligned with implementation
- Reversibility: **EASY** - Test rollback is part of phase rollback

**Supporting Research:**
- System-analysis.md observation: "Test Maintenance is Significant: Each phase must explicitly account for test updates or risk breaking CI/CD."
- Best practice is to update tests alongside implementation to avoid drift
- FlowPilot workflow expects tests to be maintained in each phase

### Option B: Dedicated Test Migration Phase

**Description:**
Complete all implementation phases (infrastructure, domain, handlers, authentication) with minimal test changes (just enough to keep CI green), then have one large phase dedicated to updating all tests comprehensively.

**Pros:**
- Can update all test infrastructure at once with consistent patterns
- Test fixtures updated once with final multi-tenant patterns
- Don't have to think about tests during implementation phases

**Cons:**
- Risk of test drift during implementation phases
- May need substantial "mock" or "hack" code to keep tests passing mid-migration
- Large test update phase is high-risk and difficult to review
- Could discover late-stage implementation issues when updating tests
- Doesn't match incremental approach from Decision 3
- High risk of breaking all tests at once

**Impact:**
- Code changes: Minimal test changes in phases 1-5, then large update in phase 6
- Test changes: All 101 test suites updated in single phase
- Risk level: **HIGH** - Single phase updating all tests is risky
- Reversibility: **MEDIUM** - Large test phase harder to rollback cleanly

**Supporting Research:**
- System-analysis.md warns: "Each phase must explicitly account for test updates or risk breaking CI/CD"
- No community examples found of successful deferred test migration approach
- Contradicts FlowPilot incremental philosophy

### Proof of Concept Required?

- [ ] Yes - Create POC to validate approach before committing
- [x] No - Decision can be made based on research and analysis

**POC Scope:** N/A - Incremental approach is clearly lower risk.

### Choice

- [x] Option A: Incremental Test Updates (Update Tests with Each Phase)
- [ ] Option B: Dedicated Test Migration Phase

**Rationale:**

1. **Best fit for this system because:** System-analysis.md identifies 101 test suites that provide confidence in correctness. Maintaining this confidence throughout migration requires keeping tests synchronized with implementation. Incremental updates ensure each phase is fully validated before proceeding.

2. **Addresses key constraints:** Must maintain CI/CD green status. Incremental test updates ensure CI stays green after each phase. Deferred test updates would require hacky mocks that keep tests passing without validating actual multi-tenant functionality.

3. **Mitigates identified risks:** System-analysis.md observation #2 states: "Test Maintenance is Significant: Each phase must explicitly account for test updates or risk breaking CI/CD." Option A directly addresses this by treating test updates as first-class work in each phase, not deferred work.

4. **Aligns with research:** FlowPilot workflow expects each phase to leave system in working, tested state. Option A matches this philosophy. References.md best practices recommend keeping tests aligned with implementation.

5. **Practical considerations:** Discovering implementation bugs during dedicated test phase (Option B) would require going back to implementation phases to fix issues, leading to back-and-forth. Option A discovers bugs immediately as each area is migrated.

### Implementation Notes

Critical guidance for implementing this decision:

- **Test fixture updates:** When adding Organization entity, immediately update all test fixtures (ArticlesFixture, ProfilesFixture) to create Organizations and associate users. Use consistent pattern across all fixtures.

- **Tenant context in tests:** Configure Finbuckle's StaticStrategy in test WebApplicationFactory setup to provide known tenant context. Each test should set a predictable TenantId (e.g., use a fixed Guid per test fixture).

- **Database wipe scripts:** Update E2E test database wipe scripts in same phase that adds Organization table. Scripts must drop Organizations table along with other tables to maintain clean test state.

- **Postman collections:** Update Postman collections as registration and authentication flows change. Update pre-request scripts to handle Organization creation semantics.

### Validation Criteria

How to confirm this decision was correct:

- **CI stays green throughout:** All test suites pass after each phase. No phases have broken tests that are "fixed later."

- **Test coverage maintained:** Code coverage metrics don't drop during migration. Tests continue validating critical functionality at each phase.

- **No test debt accumulated:** At end of final phase, no backlog of test updates exists. All tests are fully migrated and validating multi-tenant functionality.

---

## Decision 5: Tenant Resolution Strategy

### Context

**Current state:**
- Cookie-based auth (IdentityConstants.ApplicationScheme) for SPA
- Bearer token auth (IdentityConstants.BearerScheme) for API
- No current tenant resolution (system is single-tenant)

**Problem to solve:**
- Need to resolve current tenant (Organization) for each request
- Must work with both Cookie and Bearer token authentication schemes
- Must handle registration flow (new user creates new organization before authentication completes)

**Constraints:**
- Requirements specify: "Use ClaimStrategy as the primary tenant resolver. The tenant identifier claim should be: TenantId."
- Middleware ordering critical: UseMultiTenant() must come BEFORE UseAuthentication() for ClaimStrategy to work
- Registration endpoint is anonymous (no authentication before organization created)
- All other endpoints require authentication after registration

**Related systems:**
- Authentication middleware in Program.cs
- UserManager and SignInManager for cookie auth
- Token generation for bearer auth
- IClaimsTransformation for adding TenantId claim

### Options

### Option A: ClaimStrategy Only (Per Requirements)

**Description:**
Use Finbuckle's ClaimStrategy exclusively to read TenantId claim from authenticated user principal. Implement IClaimsTransformation to add TenantId claim during authentication. Registration endpoint bypasses tenant resolution (creates organization directly).

**Pros:**
- Follows requirements explicitly ("Use ClaimStrategy as the primary tenant resolver")
- Simplest configuration - single strategy
- Most secure - tenant comes from authenticated user claim (can't be spoofed)
- Natural fit for "user belongs to exactly one organization" model
- No client-side header or route management needed

**Cons:**
- Registration flow needs special handling (no claim available before authentication)
- Must implement IClaimsTransformation to add TenantId claim
- Requires careful middleware ordering (UseMultiTenant before UseAuthentication)
- Anonymous endpoints cannot resolve tenant (but none need to per requirements)

**Impact:**
- Code changes: ~50 lines for IClaimsTransformation, middleware configuration in Program.cs
- Test changes: Test fixtures use StaticStrategy (Finbuckle test pattern)
- Risk level: **MEDIUM** - Middleware ordering is critical and non-obvious
- Reversibility: **EASY** - Can add additional strategies if needed without breaking existing

**Supporting Research:**
- Goal.md explicitly specifies: "Use ClaimStrategy as the primary tenant resolver. The tenant identifier claim should be: TenantId."
- References.md section "ClaimStrategy Best Practices Discussion" confirms this is standard pattern for authenticated multi-tenant apps
- References.md section "Configuration and Usage" documents critical middleware ordering: UseMultiTenant() before UseAuthentication()

### Option B: ClaimStrategy + HeaderStrategy Fallback

**Description:**
Use ClaimStrategy as primary, but add HeaderStrategy as fallback for anonymous registration flow. Registration endpoint requires X-Organization-Id header to create organization, then returns token with claim.

**Pros:**
- Single resolution approach for all endpoints (including registration)
- Can test registration via Postman more easily (just add header)
- More flexible for future scenarios

**Cons:**
- Violates requirements (specifies ClaimStrategy only)
- Adds complexity for no current benefit (registration doesn't need tenant context)
- Security concern - headers can be spoofed by clients
- Frontend must manage header in addition to auth token
- Contradicts "user belongs to exactly one organization" model (why would client specify org?)

**Impact:**
- Code changes: ~100 lines - IClaimsTransformation + Header strategy config + validation
- Test changes: Tests must include X-Organization-Id header in registration requests
- Risk level: **MEDIUM** - Added complexity with security implications
- Reversibility: **EASY** - Can remove HeaderStrategy, keep ClaimStrategy

**Supporting Research:**
- References.md section "Alternative Approaches Considered" discusses Header-Based Tenant Resolution and concludes: "Decision: Rejected. ClaimStrategy is more secure and requires no client-side header management."
- Goal.md requirements do not mention headers or routes for tenant resolution

### Option C: ClaimStrategy with StaticStrategy Fallback for Registration

**Description:**
Use ClaimStrategy for authenticated endpoints. Use StaticStrategy with temporary tenant identifier for registration flow only. Registration endpoint uses static "registration-tenant" temporarily, then creates actual organization and re-authenticates user with proper claim.

**Pros:**
- Follows requirements (ClaimStrategy primary)
- Registration flow can use tenant context during org creation
- Clear separation between authenticated (claim-based) and registration (static) flows

**Cons:**
- Two different resolution strategies for different endpoints
- More complex configuration
- "registration-tenant" is artificial and adds conceptual overhead
- Registration flow is more complex (create org, authenticate, switch tenant)

**Impact:**
- Code changes: ~75 lines - IClaimsTransformation + Static strategy for registration endpoint
- Test changes: Tests must handle both static and claim-based strategies
- Risk level: **MEDIUM** - Two-strategy approach adds complexity
- Reversibility: **MEDIUM** - Can simplify to ClaimStrategy only, but requires refactor

**Supporting Research:**
- References.md section "Integration Testing with Finbuckle" shows StaticStrategy used for tests, not production scenarios
- No community examples found of using StaticStrategy for registration flows in production

### Proof of Concept Required?

- [ ] Yes - Create POC to validate approach before committing
- [x] No - Decision can be made based on research and analysis

**POC Scope:** N/A - Requirements are explicit about ClaimStrategy.

### Choice

- [x] Option A: ClaimStrategy Only (Per Requirements)
- [ ] Option B: ClaimStrategy + HeaderStrategy Fallback
- [ ] Option C: ClaimStrategy with StaticStrategy Fallback for Registration

**Rationale:**

1. **Best fit for this system because:** Goal.md explicitly requires: "Use ClaimStrategy as the primary tenant resolver. The tenant identifier claim should be: TenantId." The system model is "user belongs to exactly one organization," which maps naturally to claim-based resolution. Registration endpoint doesn't need tenant context (it creates the organization directly).

2. **Addresses key constraints:** ClaimStrategy works with both Cookie and Bearer token authentication schemes (both include user claims). Registration endpoint is anonymous and creates organization without needing tenant context. All post-registration endpoints are authenticated and have claim available.

3. **Mitigates identified risks:** References.md identifies middleware ordering as critical risk: "UseMultiTenant() must come BEFORE UseAuthentication() for ClaimStrategy to work." Option A's simplicity makes this easier to configure correctly. Single strategy reduces configuration complexity and potential bugs.

4. **Aligns with research:** References.md section "Alternative Approaches Considered" evaluates Header-Based and Route-Based resolution and concludes: "Decision: Rejected. ClaimStrategy is more secure and requires no client-side header management. The authenticated user's claim naturally carries the tenant identifier."

5. **Practical considerations:** Registration flow creates organization, then authenticates user with TenantId claim. All subsequent requests use claim for tenant resolution. Simple, secure, and matches requirements.

### Implementation Notes

Critical guidance for implementing this decision:

- **IClaimsTransformation implementation:** Create class that implements IClaimsTransformation to add TenantId claim after authentication. Query ApplicationUser's TenantId from UserManager and add to ClaimsPrincipal. Register as scoped service.

- **Middleware ordering:** Ensure Program.cs has: `app.UseMultiTenant()` BEFORE `app.UseAuthentication()`. This is critical for ClaimStrategy to read claims from authenticated user.

- **Registration flow:** Registration endpoint creates ApplicationUser with TenantId set. SignInManager authenticates user. IClaimsTransformation runs automatically and adds TenantId claim. Subsequent requests use claim for tenant resolution.

- **Test configuration:** Update WebApplicationFactory to use StaticStrategy with known TenantId. Don't use ClaimStrategy in tests (requires full authentication flow).

### Validation Criteria

How to confirm this decision was correct:

- **Tenant resolved for authenticated requests:** After user registers and authenticates, HttpContext.GetMultiTenantContext() returns TenantInfo with correct TenantId matching user's organization.

- **Queries automatically filtered:** After tenant resolution, EF Core queries for Articles, Comments, Tags automatically include WHERE TenantId = [current tenant]. Verified by inspecting generated SQL or query results.

- **Registration flow works:** New user can register (POST /api/identity/register), creating new Organization. Subsequent login returns token/cookie with TenantId claim. Future requests resolve tenant correctly.

---

## Decision 6: Performance & Indexing Strategy

### Context

**Current state:**
- SQLite database (development), SQL Server (production target)
- No TenantId columns or indexes
- Current queries are simple (single-tenant, no filtering)

**Problem to solve:**
- Global query filters will add WHERE TenantId = [tenant] to every query
- Need indexes on TenantId columns to prevent performance degradation
- Determine indexing strategy for all tenant-scoped tables

**Constraints:**
- Query performance must not degrade significantly after migration
- SQLite has different indexing characteristics than SQL Server
- Must support both development (SQLite) and production (SQL Server) scenarios

**Related systems:**
- All domain entities: Articles, Tags, Comments (will have TenantId)
- AspNetUsers table (will have TenantId foreign key)
- All query specifications that search tenant-scoped entities

### Options

### Option A: Single-Column Indexes on TenantId

**Description:**
Add single-column index on TenantId for every tenant-scoped table (Articles, Tags, Comments, AspNetUsers). Simple approach that covers basic filtering.

**Pros:**
- Simple to implement and maintain
- Covers the most common query pattern (filter by tenant)
- Low storage overhead
- Works well in both SQLite and SQL Server
- Easy to understand and debug

**Cons:**
- May not be optimal for queries with multiple filter criteria
- Doesn't help with queries that filter by both TenantId and another column (e.g., slug)
- Potential for larger tables to still have performance issues

**Impact:**
- Code changes: ~20 lines in entity configurations (HasIndex calls)
- Database changes: 4-5 new indexes (one per tenant-scoped table)
- Query performance: Good for basic tenant filtering
- Risk level: **LOW** - Standard indexing approach
- Reversibility: **EASY** - Can add/remove indexes without data loss

**Supporting Research:**
- References.md section "Query Filter Performance on Large Tables" notes: "Create indexes on TenantId columns for all tenant-scoped entities"
- Standard database practice for discriminator columns

### Option B: Composite Indexes on TenantId + Common Query Columns

**Description:**
Add composite indexes on (TenantId, [other column]) for common query patterns. For example: (TenantId, Slug) for Articles, (TenantId, CreatedAt) for feed queries.

**Pros:**
- Optimal performance for specific query patterns
- Can significantly speed up complex queries
- Better for production workloads with large datasets
- SQL Server can use index for both filters efficiently

**Cons:**
- More complex to maintain (must analyze query patterns)
- Higher storage overhead (multiple indexes per table)
- Risk of over-indexing (too many indexes hurt write performance)
- Requires profiling to identify optimal index combinations
- May need updates as query patterns evolve

**Impact:**
- Code changes: ~40-50 lines in entity configurations (multiple HasIndex calls)
- Database changes: 10-15 new indexes (2-3 per tenant-scoped table)
- Query performance: Excellent for targeted query patterns
- Risk level: **MEDIUM** - Complexity and potential over-indexing
- Reversibility: **EASY** - Can add/remove indexes, but more to manage

**Supporting Research:**
- References.md warns: "Monitor query execution plans" for complex queries
- Best practice is to profile first, then add composite indexes for hot paths
- Premature optimization risk

### Option C: Single-Column Indexes Now, Composite Later Based on Profiling

**Description:**
Start with single-column TenantId indexes (Option A). Profile query performance after migration. Add composite indexes later based on measured bottlenecks.

**Pros:**
- Simple initial implementation (Option A benefits)
- Data-driven approach to optimization
- Avoids premature optimization
- Can validate that single-column indexes are sufficient before adding complexity
- Incremental improvement path

**Cons:**
- May need follow-up phase to add composite indexes
- Performance testing required to identify bottlenecks
- Some queries may be slower until composite indexes added

**Impact:**
- Code changes: ~20 lines initially, more later if needed
- Database changes: 4-5 indexes now, more later if profiling identifies bottlenecks
- Query performance: Good initially, excellent after optimization
- Risk level: **LOW** - Start simple, optimize based on data
- Reversibility: **EASY** - Can always add more indexes

**Supporting Research:**
- References.md section "Query Filter Performance on Large Tables": "Monitor query execution plans"
- Standard performance optimization practice: measure, then optimize
- System is pre-production, so no real workload data yet

### Proof of Concept Required?

- [ ] Yes - Create POC to validate approach before committing
- [x] No - Decision can be made based on research and analysis

**POC Scope:** N/A - Start simple, optimize later based on data.

### Choice

- [ ] Option A: Single-Column Indexes on TenantId
- [ ] Option B: Composite Indexes on TenantId + Common Query Columns
- [x] Option C: Single-Column Indexes Now, Composite Later Based on Profiling

**Rationale:**

1. **Best fit for this system because:** System-analysis.md indicates this is pre-production application with no current workload data. Starting with simple single-column indexes covers the primary use case (tenant filtering) without premature optimization. Can add composite indexes later based on actual query patterns from real usage.

2. **Addresses key constraints:** Must prevent performance degradation from global query filters. Single-column TenantId indexes directly address this concern. SQLite (development) and SQL Server (production) both support this well.

3. **Mitigates identified risks:** References.md warns about "Query Filter Performance on Large Tables" and recommends indexing TenantId columns. Option C does this while avoiding over-indexing risk. Can monitor query plans and add targeted composite indexes if bottlenecks identified.

4. **Aligns with research:** References.md section "Query Filter Performance on Large Tables" states: "Monitor query execution plans" - this implies measure-then-optimize approach. Standard practice is to start simple, profile, then optimize hot paths.

5. **Practical considerations:** Pre-production system has no real workload to profile yet. Starting simple (Option A) gets migration complete faster. Can add composite indexes in follow-up phase once production usage provides real performance data. Avoids maintenance burden of multiple indexes until proven necessary.

### Implementation Notes

Critical guidance for implementing this decision:

- **Index definition:** Use fluent API in entity configurations: `builder.HasIndex(e => e.TenantId).HasDatabaseName("IX_[Table]_TenantId")`

- **Index on all tenant-scoped entities:** Add index to: Articles, Tags, Comments, AspNetUsers. Do NOT add to join tables (ArticleTags, ArticleFavorites) unless profiling shows bottleneck.

- **Monitor query plans:** After migration, use EF Core logging to log generated SQL. Check that queries use TenantId index. Look for table scans indicating missing indexes.

- **Profiling strategy:** Use SQL Server Management Studio (production) or SQLite query analyzer (development) to examine execution plans for common queries: list articles, get feed, search tags.

### Validation Criteria

How to confirm this decision was correct:

- **Indexes created:** Migration creates TenantId indexes on Articles, Tags, Comments, AspNetUsers tables. Verify with database inspection tools.

- **Queries use indexes:** EF Core generated SQL includes WHERE TenantId = [tenant] and database uses index (not table scan). Verify with query execution plans.

- **No performance degradation:** After migration, response times for list/search endpoints remain similar to pre-migration baseline (within 10-20%). Measure with Postman or functional tests.

- **Follow-up optimization path:** If performance bottlenecks identified via profiling, clear path exists to add composite indexes (e.g., on Slug, CreatedAt, AuthorId) without schema refactoring.

---