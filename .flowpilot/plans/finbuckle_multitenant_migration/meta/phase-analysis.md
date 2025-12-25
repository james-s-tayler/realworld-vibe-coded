
1. **Phase sizes**: All phases are Small (5-10 steps). This is acceptable as smaller phases are easier to manage and revert.

2. **Working state transitions**: Each phase explicitly defines the transition from one working state to another. Phase 1 is isolated POC. Phases 2-9 progressively build multi-tenant functionality while keeping system in working state. Phases 10-14 add comprehensive test coverage and logging.

3. **High-risk decomposition**: The highest-risk item (DbContext + Audit.NET integration from Decision 1) is split into phase_1 (POC validation) and phase_4 (implementation). Phase_8 (tenant resolution with middleware ordering) is kept small to manage risk.

4. **Test maintenance**: Tests are updated incrementally throughout phases. Phase 10 updates E2E test infrastructure for per-test org registration. Phases 11-13 add comprehensive multi-tenant test coverage. This follows Decision 4 (incremental test updates).

5. **Rollback feasibility**: Each phase is independently committable. Phase 1 is isolated POC. Phases 2-9 use EF migrations which are reversible. Phases 10-14 are additive test/logging improvements. No phase has destructive changes that prevent rollback.

6. **Dependencies**: Linear progression - each phase depends only on previous phase completing. No complex dependency graphs. Phase 1 validates Decision 1 before proceeding.

7. **Risk progression**: Starts with low-risk POC (phase_1), then frontend changes (phase_2-3), infrastructure (phase_4-5), authentication changes (phase_6-7), tenant resolution (phase_8), handler updates (phase_9), test infrastructure and coverage (phases_10-13), and ends with low-risk logging (phase_14).

8. **Alignment with key decisions**:
   - Decision 1 (DbContext strategy): Validated in phase_1, implemented in phase_4
   - Decision 2 (Data migration): Clean slate approach throughout all phases
   - Decision 3 (Incremental phases): 14 small phases as planned
   - Decision 4 (Test strategy): Incremental updates in phases 2-13
   - Decision 5 (ClaimStrategy): Implemented in phase_8
   - Decision 6 (Indexing): Single-column indexes in phases 4 and 5

### phase_1

**Goal**: Create proof-of-concept validating MultiTenantIdentityDbContext + Audit.NET integration

**Key Outcomes**:
* POC demonstrates AppDbContext can inherit from MultiTenantIdentityDbContext while maintaining Audit.NET functionality
* Verify query filters automatically apply to tenant-scoped entities
* Confirm Audit.NET data provider captures TenantId in audit events
* Validate both concerns work together without conflicts
* Decision 1 from key-decisions.md is validated or adjusted based on POC findings

**Working State Transition**: From current working state (AuditIdentityDbContext) to validated POC that proves MultiTenantIdentityDbContext + Audit.NET integration is viable. POC code is in separate branch/folder and does not affect main codebase. All existing tests continue to pass.

**Scope Size:** Small (POC only, ~5-8 steps)
**Risk Level:** Low (isolated POC, no production code changes)
**Dependencies:** None (first phase)
**Ripple Effects:** None yet (POC is isolated)

---

### phase_2

**Goal**: Update frontend to remove all unauthenticated user access to data

**Key Outcomes**:
* Home page redirects unauthenticated users to login/register
* Remove global feed view for unauthenticated users
* Remove article detail view for unauthenticated users
* Remove profile view for unauthenticated users
* All frontend routes require authentication
* Frontend tests updated for auth-required behavior

**Working State Transition**: From current state where unauthenticated users can browse global feed and view articles to state where all data viewing requires authentication. This prepares the application for multi-tenancy where query filters will make unauthenticated data access impossible. Frontend still works with current backend (no breaking changes yet). All frontend tests pass.

**Scope Size:** Small (~8 steps)
**Risk Level:** Low (frontend-only changes, no backend impact)
**Dependencies:** phase_1 completed
**Ripple Effects:**
- Frontend routing configuration
- Home page component
- Navigation components
- ~10-15 frontend component tests
- E2E tests that assume unauthenticated browsing

---

### phase_3

**Goal**: Update backend to require authentication on all endpoints and update Postman collections

**Key Outcomes**:
* All GET endpoints for articles, comments, profiles, tags require authentication
* Endpoints return 401 Unauthorized when accessed without auth
* Postman collections updated to expect 401 on unauthenticated requests
* Postman collections pass with auth tokens
* Functional tests updated for auth-required endpoints
* E2E tests pass with auth-required backend

**Working State Transition**: From frontend requiring auth to full stack requiring auth. No unauthenticated users can access any data. This completes the prerequisite for multi-tenancy migration (query filters won't break unauthenticated access because it no longer exists). All tests pass with authentication required throughout.

**Scope Size:** Small (~10 steps)
**Risk Level:** Medium (breaking API change, affects all clients)
**Dependencies:** phase_2 completed
**Ripple Effects:**
- FastEndpoints configuration for all GET endpoints
- Postman collections (~5 collections)
- Functional tests (~45 tests may need auth setup)
- E2E tests (~51 tests)

---

### phase_4

**Goal**: Add Organization entity and EF Core infrastructure for multi-tenancy

**Key Outcomes**:
* Organization entity created with required properties (Id, Name, Identifier, audit fields)
* AppDbContext migrated from AuditIdentityDbContext to MultiTenantIdentityDbContext inheritance
* Audit.NET configured via data provider to work with new DbContext inheritance
* TenantId added to ApplicationUser entity with foreign key relationship
* EF Core migrations created for Organization table and ApplicationUser.TenantId column
* Single-column indexes added to TenantId columns per Decision 6
* All existing functional tests pass with updated fixtures

**Working State Transition**: From POC validated to multi-tenant infrastructure in place. Organization table exists, users can have TenantId, but no data isolation yet enforced. Application still works as single-tenant while infrastructure is prepared. All tests pass with minimal fixture updates.

**Scope Size:** Small (~10 steps)
**Risk Level:** Medium (DbContext inheritance change is critical)
**Dependencies:** phase_3 completed
**Ripple Effects:** 
- AppDbContext.cs (inheritance change, Audit.NET config)
- ApplicationUser entity and configuration
- Database migrations
- Test fixtures (create Organizations for test users)
- ~5 functional test files

---

### phase_5

**Goal**: Add TenantId to domain entities and mark as multi-tenant

**Key Outcomes**:
* EntityBase class marked with `[MultiTenant]` attribute (Article, Tag, Comment inherit this automatically)
* TenantId property added to EntityBase with EF configuration and index
* EF Core migrations created for schema changes
* Global query filters automatically apply to all tenant-scoped entities
* Existing handlers and queries work without modification (filters auto-apply)
* Functional tests updated for tenant-aware queries

**Working State Transition**: From multi-tenant infrastructure in place to domain entities being tenant-scoped. Query filters automatically restrict data by TenantId. However, entities don't have TenantId values yet (will be set in later phases), so queries return empty results. Tests may need adjustments to account for filtering. System is in valid state with schema ready for tenant data.

**Scope Size:** Small (~8 steps)
**Risk Level:** Low (additive changes, filters are automatic)
**Dependencies:** phase_4 completed  
**Ripple Effects:**
- EntityBase class and configuration
- Article, Tag, Comment entities (inherit from EntityBase)
- Database migrations  
- Potentially some specifications if they bypass filters
- ~10-15 functional tests may need fixture updates

**Note**: By marking EntityBase with the `[MultiTenant]` attribute, all entities that inherit from it (Article, Tag, Comment) automatically become tenant-scoped without needing individual attribute decorations.

---

### phase_6

**Goal**: Create custom registration and login endpoints as pure refactoring (existing behavior)

**Key Outcomes**:
* Custom `/api/identity/register` endpoint created replacing default ASP.NET Identity endpoint
* Custom `/api/identity/login` endpoint created replacing default ASP.NET Identity endpoint
* Both endpoints implement exactly the same behavior as default endpoints (pure refactoring)
* All existing tests pass without modification
* Postman collections work with new endpoints
* E2E tests work with new endpoints

**Working State Transition**: From using default ASP.NET Identity endpoints to custom endpoints at `/api/identity/register` and `/api/identity/login` with identical behavior. This prepares for behavior changes in next phase (Organization creation, claims transformation) while keeping system in working state. All tests pass.

**Scope Size:** Small (~8 steps)
**Risk Level:** Medium (authentication is critical, but pure refactoring reduces risk)
**Dependencies:** phase_5 completed
**Ripple Effects:**
- New registration endpoint/handler
- New login endpoint/handler
- Program.cs (endpoint mapping)
- Potentially frontend API client configuration
- E2E tests (endpoint URLs)

---

### phase_7

**Goal**: Update registration to create Organization and login to add TenantId claim

**Key Outcomes**:
* Registration endpoint creates new Organization when user registers
* User is associated with newly created Organization (TenantId foreign key)
* User is assigned "Owner" role for their Organization
* Login endpoint adds TenantId claim during sign-in
* IClaimsTransformation includes TenantId in claims after authentication
* Registration and login tests updated for new behavior
* E2E tests updated for Organization creation semantics

**Working State Transition**: From custom endpoints with existing behavior to endpoints that create Organizations and add tenant claims. New users create Organizations and become Owners. All subsequent requests have TenantId claim. This enables tenant resolution in next phase. All tests pass with updated Organization creation logic.

**Scope Size:** Small (~10 steps)
**Risk Level:** Medium (registration and auth flow changes are critical)
**Dependencies:** phase_6 completed
**Ripple Effects:**
- Registration handler (create Organization, assign Owner role)
- Login handler/IClaimsTransformation (add TenantId claim)
- Registration tests (~5-8 tests)
- Login tests (~3-5 tests)
- E2E registration/login tests (~5-8 tests)

---

### phase_8

**Goal**: Implement tenant resolution via ClaimStrategy

**Key Outcomes**:
* Custom TenantInfo class created mapping to Organization entity
* Finbuckle.MultiTenant configured with ClaimStrategy in Program.cs
* Middleware ordering configured: UseMultiTenant() before UseAuthentication()
* EF Core store configured for tenant resolution
* Tenant context accessible via IMultiTenantContextAccessor
* Functional tests updated to use StaticStrategy for known tenant context
* Query filters now work end-to-end (tenant resolved → filters applied)

**Working State Transition**: From TenantId claims being added to full tenant resolution working. After authentication, TenantId claim is present and Finbuckle resolves current tenant. Query filters now work end-to-end automatically filtering by tenant. Tests use StaticStrategy to simulate tenant context. Data isolation is now enforced on all queries.

**Scope Size:** Small (~8 steps)
**Risk Level:** High (middleware ordering is critical, tenant resolution is core functionality)
**Dependencies:** phase_7 completed
**Ripple Effects:**
- Program.cs (DI configuration, middleware ordering)
- Custom TenantInfo class
- All functional test fixtures (StaticStrategy setup)
- ~45 functional tests

---

### phase_9

**Goal**: Update handlers to set TenantId when creating tenant-scoped entities

**Key Outcomes**:
* CreateArticleHandler sets Article.TenantId from current tenant context
* CreateCommentHandler sets Comment.TenantId from current tenant context
* Tag creation (within article creation) sets Tag.TenantId from current tenant context
* All create operations properly associate entities with current organization
* Verify ownership checks work within organization boundaries
* Tests validate entities are created with correct TenantId

**Working State Transition**: From tenant resolution working to all entity creation operations being tenant-aware. Articles, Comments, and Tags are created with TenantId set. Data isolation is enforced both on read (query filters) and write (explicit TenantId assignment). System is fully multi-tenant with complete CRUD isolation.

**Scope Size:** Small (~6 steps)
**Risk Level:** Low (straightforward context propagation)
**Dependencies:** phase_8 completed
**Ripple Effects:**
- CreateArticleHandler
- CreateCommentHandler
- Tag creation logic (if separate)
- ~5-8 handler tests

---

### phase_10

**Goal**: Update E2E test infrastructure for per-test organization registration and parallel execution

**Key Outcomes**:
* E2E test infrastructure updated to register new organizations per-test
* Tests no longer require database wipe between executions
* Tests can run in parallel without data interference
* Helper methods added for creating isolated test users/organizations
* Existing E2E tests updated to use per-test org registration pattern
* Test execution time improves due to parallelization

**Working State Transition**: From E2E tests using database wipe for isolation to tests using per-test organization registration. Each test creates its own organization(s), enabling parallel execution. Database wipe is no longer required. Tests run faster and are more isolated.

**Scope Size:** Small (~7 steps)
**Risk Level:** Low (test infrastructure improvements, no production code changes)
**Dependencies:** phase_9 completed
**Ripple Effects:**
- E2E test base classes (helper methods for org registration)
- All existing E2E tests (update to use per-test org registration)
- Test configuration (enable parallel execution)
- Database wipe scripts (deprecated or removed)

---

### phase_11

**Goal**: Add E2E tests for multi-tenancy semantics and cross-tenant isolation

**Key Outcomes**:
* E2E tests added verifying users in different organizations cannot see each other's data
* E2E tests verify articles, comments, tags are properly scoped to tenant
* E2E tests verify follow relationships work within organization boundaries
* Tests use per-test org registration from phase_10 for isolation
* All new E2E tests pass

**Working State Transition**: From fully functional multi-tenant system to system with comprehensive E2E test coverage for multi-tenancy. E2E tests validate data isolation across multiple organizations. Regression prevention for tenant isolation bugs is in place.

**Scope Size:** Small (~7 steps)
**Risk Level:** Low (additive test coverage)
**Dependencies:** phase_10 completed
**Ripple Effects:**
- New E2E test files for multi-tenancy scenarios
- E2E test fixtures

---

### phase_12

**Goal**: Update slug uniqueness validation and tests

**Key Outcomes**:
* Article slug uniqueness validation updated to be unique within tenant (not globally)
* Slug validation tests updated for tenant-scoped uniqueness
* Tests verify same slug can exist in different organizations
* Functional tests for slug uniqueness pass

**Working State Transition**: From global slug uniqueness to tenant-scoped slug uniqueness. Articles in different organizations can have the same slug. Validation logic and tests reflect multi-tenant slug semantics.

**Scope Size:** Small (~5 steps)
**Risk Level:** Low (validation logic update)
**Dependencies:** phase_11 completed
**Ripple Effects:**
- Slug validation logic
- Article creation validator
- ~3-5 slug uniqueness tests

---

### phase_13

**Goal**: Add functional tests for multi-tenancy semantics and cross-tenant isolation

**Key Outcomes**:
* Functional tests added verifying tenant isolation at handler/repository level
* Tests verify query filters work correctly for articles, comments, tags
* Tests verify create operations set TenantId correctly
* Tests verify users cannot access other tenants' data
* All functional tests pass

**Working State Transition**: From E2E test coverage to comprehensive functional test coverage for multi-tenancy. Functional tests validate data isolation at the handler and repository level. Complete test coverage for multi-tenant scenarios.

**Scope Size:** Small (~8 steps)
**Risk Level:** Low (additive test coverage)
**Dependencies:** phase_12 completed
**Ripple Effects:**
- New functional test files for multi-tenancy scenarios
- Test fixtures for multi-tenant test scenarios

---

### phase_14

**Goal**: Add Serilog enricher for tenant context logging

**Key Outcomes**:
* Serilog configured with tenant context enricher
* TenantId automatically included in all log entries
* Logs can be filtered by tenant for debugging
* Audit.NET events include TenantId (configured in phase_4)
* Manual verification of logs confirms tenant context present

**Working State Transition**: From fully functional multi-tenant system to system with comprehensive tenant-aware logging and auditing. All logs and audit events include TenantId for debugging and compliance. System is production-ready with observability for multi-tenant scenarios.

**Scope Size:** Small (~5 steps)
**Risk Level:** Low (logging enrichment is additive)
**Dependencies:** phase_13 completed
**Ripple Effects:**
- Program.cs or LoggerConfig.cs (Serilog configuration)
- Potentially custom Serilog enricher class
- Log verification (manual check of Logs/** directory)

---

## Phase Validation Checklist

Before finalizing the phase analysis, verify:

- [x] Each phase has 3-5 key outcomes (not too many, not too few)
- [x] Each phase has a clear working state transition
- [x] No phase is too large (>20 steps indicates need to split)
- [x] High-risk phases are broken into smaller increments
- [x] Test maintenance is accounted for in phases
- [x] Rollback is possible at the end of any phase
- [x] Dependencies between phases are clear and minimal
- [x] The sequence progresses from low-risk to high-risk where possible

**Validation Notes:**

1. **Phase sizes**: All phases are Small (5-10 steps). This ensures manageable scope and easy rollback.

2. **Working state transitions**: Each phase explicitly defines the transition from one working state to another. Phase 1 validates the POC first. Phases 2-3 prepare the system for multi-tenancy by requiring authentication everywhere (prerequisite for query filters). Phases 4-13 progressively build multi-tenant functionality while keeping system in working state.

3. **High-risk decomposition**: 
   - POC validation done first in phase 1 (lowest risk, validates Decision 1)
   - Authentication changes split into phases 2-3 (frontend first, then backend)
   - The highest-risk item (DbContext + Audit.NET integration from Decision 1) is split into phase_1 (POC validation) and phase_4 (implementation)
   - Registration/login changes split into phases 6-7 (refactoring first, then behavior change)
   - Tenant resolution (phase_8) kept small despite high risk
   - Test updates split into phases 10, 11, 12 instead of one large phase

4. **Test maintenance**: Tests updated incrementally throughout:
   - Phase 1: POC isolated, no test changes
   - Phase 2: Frontend tests
   - Phase 3: Backend functional tests, Postman, E2E tests  
   - Phases 4-9: Functional tests as fixtures/handlers affected
   - Phase 10: E2E multi-tenancy tests
   - Phase 11: Slug uniqueness tests
   - Phase 12: Functional multi-tenancy tests
   This follows Decision 4 (incremental test updates).

5. **Rollback feasibility**: Each phase is independently committable. Phase 1 is isolated POC. Phases 4-13 use EF migrations which are reversible. No phase has destructive changes that prevent rollback.

6. **Dependencies**: Linear progression - each phase depends only on previous phase completing. Phase 1 validates Decision 1 before phase 4 proceeds. Phases 2-3 complete auth requirements before multi-tenant work begins.

7. **Risk progression**: 
   - Starts with low-risk POC validation (phase_1)
   - Low-risk frontend changes (phase_2)
   - Medium-risk backend auth changes (phase_3)
   - Medium-risk infrastructure (phase_4)
   - Low-risk domain entities (phase_5)
   - Medium-risk auth refactoring (phase_6)
   - Medium-risk auth behavior changes (phase_7)
   - High-risk tenant resolution (phase_8)
   - Low-risk handler updates (phase_9)
   - Low-risk test additions (phases_10-12)
   - Low-risk logging (phase_13)

8. **Alignment with key decisions**:
   - Decision 1 (DbContext strategy): Validated in phase_1, implemented in phase_4
   - Decision 2 (Data migration): Clean slate approach throughout all phases
   - Decision 3 (Incremental phases): 13 small phases as planned
   - Decision 4 (Test strategy): Incremental updates in phases 2-3, 4-12
   - Decision 5 (ClaimStrategy): Implemented in phase_8
   - Decision 6 (Indexing): Single-column indexes in phases 4-5

9. **New phases rationale**:
   - Phase 1: POC moved to beginning per feedback to validate approach before any other changes
   - Phases 2-3 added to address requirement that unauthenticated data access must be removed before multi-tenancy (query filters would break unauthenticated browsing)
   - Phase 6-7 split added per feedback to separate refactoring from behavior change in auth flows
   - Phase 10-12 split added per feedback to separate E2E, slug, and functional test updates

10. **Postman maintenance**: Postman collections are maintained in phase 3 when backend auth requirements change. No additional Postman-specific multi-tenancy testing phases as per feedback (only necessary maintenance, not expanded coverage).
