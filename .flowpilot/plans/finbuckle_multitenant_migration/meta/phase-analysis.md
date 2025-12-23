## Phase Analysis

<!--
FlowPilot parsing rules (v1):

- A phase starts with "### phase_<number>"
- The number defines ordering
- Sections under each phase:
    - **Goal**: <text>
    - **Key Outcomes**: list of "* outcome"
    - **Working State Transition**: <text>

The linter and next-command can rely on these headings exactly.
-->

## Phase Planning Principles

Before defining phases, ensure:

1. **Each phase achieves a complete, working state** - No phase should leave the system in a broken state
2. **Phases are independently testable** - Each phase has clear pass/fail criteria
3. **Phase size is manageable** - Target 5-15 implementation steps per phase
4. **Dependencies are explicit** - Each phase clearly states what must be done first
5. **Scope is minimal** - Each phase does the minimum needed to reach next working state
6. **Risks are identified early** - High-risk phases are broken into smaller phases
7. **Test maintenance is considered** - Account for test updates in phase scope

## Phase Scope Guidelines

**Small Phase (Recommended):**
- 5-10 implementation steps
- 1-2 hours of work
- Affects 5-15 files
- Low-medium risk
- Easy to rollback

**Medium Phase (Use with caution):**
- 10-20 implementation steps
- 2-4 hours of work
- Affects 15-30 files
- Medium risk
- Requires careful planning

**Large Phase (Avoid if possible):**
- 20+ implementation steps
- 4+ hours of work
- Affects 30+ files
- High risk
- Should be split into smaller phases

## Ripple Effect Analysis

Before finalizing phases, analyze ripple effects:

**If changing database schema:**
- [ ] Entity classes affected
- [ ] Repository methods affected
- [ ] Handlers affected
- [ ] API contracts affected
- [ ] Tests affected
- [ ] Migrations needed

**If changing authentication:**
- [ ] Middleware affected
- [ ] Endpoints affected
- [ ] Test fixtures affected
- [ ] Client code affected
- [ ] Cookie/token handling affected

**If changing domain entities:**
- [ ] Handlers querying the entity
- [ ] Mappers using the entity
- [ ] Specifications using the entity
- [ ] Tests using the entity
- [ ] Related entities affected

## Phase Definitions

### phase_1

**Goal**: [One sentence describing the phase goal]

**Key Outcomes**:
* Outcome 1 - specific, measurable
* Outcome 2 - specific, measurable
* Outcome 3 - specific, measurable

**Working State Transition**: [Describe what changes from working state A to working state B. Both states must be working states with all tests passing]

**Scope Size:** Small/Medium/Large
**Risk Level:** Low/Medium/High
**Dependencies:** [List phases or external work that must complete first]
**Ripple Effects:** [List areas of code that will be affected]

---

### phase_2

**Goal**: [One sentence describing the phase goal]

**Key Outcomes**:
* Outcome 1 - specific, measurable
* Outcome 2 - specific, measurable

**Working State Transition**: [Describe the working state transition]

**Scope Size:** Small/Medium/Large
**Risk Level:** Low/Medium/High
**Dependencies:** [Typically "phase_1 completed"]
**Ripple Effects:** [List areas of code that will be affected]

---

[Continue with additional phases...]

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

1. **Phase sizes**: All phases are Small (5-10 steps) except phase_7 which is Medium (12-15 steps) due to comprehensive test updates. This is acceptable as test updates are lower risk than infrastructure changes.

2. **Working state transitions**: Each phase explicitly defines the transition from one working state to another. Phase 1 is isolated POC. Phases 2-8 progressively build multi-tenant functionality while keeping system in working state.

3. **High-risk decomposition**: The highest-risk item (DbContext + Audit.NET integration from Decision 1) is split into phase_1 (POC validation) and phase_2 (implementation). Phase_4 (tenant resolution with middleware ordering) is kept small to manage risk.

4. **Test maintenance**: Tests are updated incrementally throughout phases 2-7. Functional tests updated as fixtures are affected (phases 2-6). Comprehensive E2E and Postman updates in dedicated phase_7. This follows Decision 4 (incremental test updates).

5. **Rollback feasibility**: Each phase is independently committable. Phase 1 is isolated POC. Phases 2-8 use EF migrations which are reversible. No phase has destructive changes that prevent rollback.

6. **Dependencies**: Linear progression - each phase depends only on previous phase completing. No complex dependency graphs. Phase 1 validates Decision 1 before phase 2 proceeds.

7. **Risk progression**: Starts with low-risk POC (phase_1), then infrastructure (phase_2), domain entities (phase_3), builds to high-risk tenant resolution (phase_4), then registration flow (phase_5), handler updates (phase_6), comprehensive tests (phase_7), and ends with low-risk logging (phase_8).

8. **Alignment with key decisions**:
   - Decision 1 (DbContext strategy): Validated in phase_1, implemented in phase_2
   - Decision 2 (Data migration): Clean slate approach throughout all phases
   - Decision 3 (Incremental phases): 8 small phases as planned
   - Decision 4 (Test strategy): Incremental updates in phases 2-7
   - Decision 5 (ClaimStrategy): Implemented in phase_4
   - Decision 6 (Indexing): Single-column indexes in phase_2 and phase_3

### phase_1

**Goal**: Create proof-of-concept validating MultiTenantIdentityDbContext + Audit.NET integration

**Key Outcomes**:
* POC demonstrates AppDbContext can inherit from MultiTenantIdentityDbContext while maintaining Audit.NET functionality
* Verify query filters automatically apply to tenant-scoped entities
* Confirm Audit.NET data provider captures OrganizationId in audit events
* Validate both concerns work together without conflicts
* Decision 1 from key-decisions.md is validated or adjusted based on POC findings

**Working State Transition**: From current working state (AuditIdentityDbContext) to validated POC that proves MultiTenantIdentityDbContext + Audit.NET integration is viable. POC code is in separate branch/folder and does not affect main codebase. All existing tests continue to pass.

**Scope Size:** Small (POC only, ~5-8 steps)
**Risk Level:** Low (isolated POC, no production code changes)
**Dependencies:** None (first phase)
**Ripple Effects:** None yet (POC is isolated)

---

### phase_2

**Goal**: Add Organization entity and EF Core infrastructure for multi-tenancy

**Key Outcomes**:
* Organization entity created with required properties (Id, Name, Identifier, audit fields)
* AppDbContext migrated from AuditIdentityDbContext to MultiTenantIdentityDbContext inheritance
* Audit.NET configured via data provider to work with new DbContext inheritance
* OrganizationId added to ApplicationUser entity with foreign key relationship
* EF Core migrations created for Organization table and ApplicationUser.OrganizationId column
* Single-column indexes added to OrganizationId columns per Decision 6
* All existing functional tests pass with updated fixtures

**Working State Transition**: From current single-tenant schema to multi-tenant infrastructure in place. Organization table exists, users can have OrganizationId, but no data isolation yet enforced. Application still works as single-tenant while infrastructure is prepared. All tests pass with minimal fixture updates.

**Scope Size:** Small (~10 steps)
**Risk Level:** Medium (DbContext inheritance change is critical)
**Dependencies:** phase_1 completed and POC successful
**Ripple Effects:** 
- AppDbContext.cs (inheritance change, Audit.NET config)
- ApplicationUser entity and configuration
- Database migrations
- Test fixtures (create Organizations for test users)
- ~5 functional test files

---

### phase_3

**Goal**: Add OrganizationId to domain entities (Article, Tag, Comment) and mark as multi-tenant

**Key Outcomes**:
* Article entity has OrganizationId property with EF configuration and index
* Tag entity has OrganizationId property with EF configuration and index  
* Comment entity has OrganizationId property with EF configuration and index
* Entities marked with `[MultiTenant]` attribute or `.IsMultiTenant()` fluent API
* EF Core migrations created for schema changes
* Global query filters automatically apply to all tenant-scoped entities
* Existing handlers and queries work without modification (filters auto-apply)

**Working State Transition**: From multi-tenant infrastructure in place to domain entities being tenant-scoped. Query filters automatically restrict data by OrganizationId. However, entities don't have OrganizationId values yet (will be set in phase_4), so queries return empty results. Tests may need adjustments to account for filtering. System is in valid state with schema ready for tenant data.

**Scope Size:** Small (~8 steps)
**Risk Level:** Low (additive changes, filters are automatic)
**Dependencies:** phase_2 completed  
**Ripple Effects:**
- Article, Tag, Comment entity classes and configurations
- Database migrations  
- Potentially some specifications if they bypass filters
- ~10-15 functional tests may need fixture updates

---

### phase_4

**Goal**: Implement tenant resolution via ClaimStrategy and IClaimsTransformation

**Key Outcomes**:
* Custom TenantInfo class created mapping to Organization entity
* Finbuckle.MultiTenant configured with ClaimStrategy in Program.cs
* IClaimsTransformation implemented to add OrganizationId claim after authentication
* Middleware ordering configured: UseMultiTenant() before UseAuthentication()
* EF Core store configured for tenant resolution
* Tenant context accessible via IMultiTenantContextAccessor
* Functional tests updated to use StaticStrategy for known tenant context

**Working State Transition**: From entities marked as tenant-scoped to full tenant resolution working. After authentication, OrganizationId claim is present and Finbuckle resolves current tenant. Query filters now work end-to-end (tenant resolved → filters applied). Tests use StaticStrategy to simulate tenant context. System can resolve tenants but registration flow doesn't create Organizations yet.

**Scope Size:** Small (~10 steps)
**Risk Level:** High (middleware ordering is critical, authentication integration complex)
**Dependencies:** phase_3 completed
**Ripple Effects:**
- Program.cs (DI configuration, middleware ordering)
- New IClaimsTransformation implementation
- Custom TenantInfo class
- All functional test fixtures (StaticStrategy setup)
- ~45 functional tests

---

### phase_5

**Goal**: Update registration flow to create Organization and assign Owner role

**Key Outcomes**:
* Registration endpoint creates new Organization when user registers
* User is associated with newly created Organization (OrganizationId foreign key)
* User is assigned "Owner" role for their Organization
* OrganizationId claim is added during registration sign-in
* IClaimsTransformation includes OrganizationId in claims after registration
* Registration flow tests updated for new behavior
* E2E tests updated for Organization creation semantics

**Working State Transition**: From tenant resolution working to full registration-to-organization flow complete. New users create Organizations and become Owners. All subsequent requests have tenant context. Data isolation is fully functional. System is ready for multi-tenant usage with registration as entry point.

**Scope Size:** Small (~8 steps)
**Risk Level:** Medium (registration is critical path, affects user flow)
**Dependencies:** phase_4 completed
**Ripple Effects:**
- Registration handler/endpoint
- Sign-in logic (add OrganizationId claim)
- Owner role assignment
- Registration tests (~5-8 tests)
- E2E registration tests (~3-5 tests)

---

### phase_6

**Goal**: Update handlers to set OrganizationId when creating tenant-scoped entities

**Key Outcomes**:
* CreateArticleHandler sets Article.OrganizationId from current tenant context
* CreateCommentHandler sets Comment.OrganizationId from current tenant context
* Tag creation (within article creation) sets Tag.OrganizationId from current tenant context
* All create operations properly associate entities with current organization
* Verify ownership checks work within organization boundaries
* Tests validate entities are created with correct OrganizationId

**Working State Transition**: From registration creating Organizations to all entity creation operations being tenant-aware. Articles, Comments, and Tags are created with OrganizationId set. Data isolation is enforced both on read (query filters) and write (explicit OrganizationId assignment). System is fully multi-tenant with complete CRUD isolation.

**Scope Size:** Small (~6 steps)
**Risk Level:** Low (straightforward context propagation)
**Dependencies:** phase_5 completed
**Ripple Effects:**
- CreateArticleHandler
- CreateCommentHandler
- Tag creation logic (if separate)
- ~5-8 handler tests

---

### phase_7

**Goal**: Update remaining tests (E2E, Postman) and verify end-to-end multi-tenancy

**Key Outcomes**:
* All 51 E2E tests updated for multi-tenant behavior
* Database wipe scripts handle Organizations table
* All 5 Postman collections updated for Organization semantics
* Postman environment variables and pre-request scripts updated
* Cross-tenant isolation tests added (verify users can't see other org's data)
* Slug uniqueness tests updated (unique within org, not globally)
* All test suites pass: functional (45), E2E (51), Postman (5)

**Working State Transition**: From core multi-tenancy implementation complete to full test coverage validating multi-tenant isolation. All test suites comprehensively validate that data is scoped to organizations and no cross-tenant leakage exists. System is production-ready for multi-tenant usage with complete test coverage.

**Scope Size:** Medium (~12-15 steps)
**Risk Level:** Medium (comprehensive test updates, many files)
**Dependencies:** phase_6 completed
**Ripple Effects:**
- All 51 E2E tests
- Database wipe scripts  
- 5 Postman collections
- Postman environment/pre-request scripts
- New isolation tests

---

### phase_8

**Goal**: Add Serilog enricher for tenant context logging

**Key Outcomes**:
* Serilog configured with tenant context enricher
* OrganizationId automatically included in all log entries
* Logs can be filtered by tenant for debugging
* Audit.NET events include OrganizationId (configured in phase_2)
* Manual verification of logs confirms tenant context present
* Documentation updated with tenant logging approach

**Working State Transition**: From fully functional multi-tenant system to system with comprehensive tenant-aware logging and auditing. All logs and audit events include OrganizationId for debugging and compliance. System is production-ready with observability for multi-tenant scenarios.

**Scope Size:** Small (~5 steps)
**Risk Level:** Low (logging enrichment is additive)
**Dependencies:** phase_7 completed
**Ripple Effects:**
- Program.cs or LoggerConfig.cs (Serilog configuration)
- Potentially custom Serilog enricher class
- Log verification (manual check of Logs/** directory)

---