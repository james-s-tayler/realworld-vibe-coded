## phase_1: POC Validation for MultiTenantIdentityDbContext + Audit.NET Integration

### Phase Overview

Create a proof-of-concept to validate that `MultiTenantIdentityDbContext` from Finbuckle can coexist with Audit.NET's data provider approach. This validates Decision 1 from key-decisions.md before committing to the full migration.

**Scope Size:** Small (~5-8 steps)
**Risk Level:** Low (isolated POC, no production code changes)
**Estimated Complexity:** Low

### Prerequisites

What must be completed before starting this phase:
- All existing tests passing (baseline: 45 functional, 51 E2E, 5 Postman collections)
- Development environment set up with .NET 9 SDK
- Familiarity with references.md, system-analysis.md, and key-decisions.md

### Known Risks & Mitigations

**Risk 1:** MultiTenantIdentityDbContext and Audit.NET data provider may have incompatible behaviors
- **Likelihood:** Low
- **Impact:** High (would require rethinking entire approach from Decision 1)
- **Mitigation:** This POC is specifically designed to validate the approach before any production code changes. Use a separate branch or folder for POC code.
- **Fallback:** If incompatibility found, evaluate Decision 1 Option A (manual implementation) or Option C (custom multi-tenancy). Run `flowpilot stuck` to re-evaluate.

**Risk 2:** TenantId may not be captured in Audit.NET events
- **Likelihood:** Low
- **Impact:** Medium (audit compliance requirement)
- **Mitigation:** Explicitly configure Audit.NET data provider to include TenantId from IMultiTenantContextAccessor
- **Fallback:** Add custom audit event enricher to inject TenantId if automatic capture fails

### Implementation Steps

**Part 1: Setup & Preparation**

1. **Create POC folder**
   - Create folder `Task/PoC/FinbuckleMultitenantAuditNet` for the POC project
   - This keeps POC code isolated from production codebase
   - Expected outcome: Clean workspace for POC experimentation
   - Files affected: `Task/PoC/FinbuckleMultitenantAuditNet/`

2. **Install Finbuckle.MultiTenant package**
   - Add NuGet package `Finbuckle.MultiTenant.EntityFrameworkCore` version 10.0+ to POC project
   - Expected outcome: Package reference added successfully
   - Reality check: Run `dotnet restore` and verify no dependency conflicts

**Part 2: Core Changes**

3. **Create minimal POC DbContext**
   - Create `PocDbContext` that inherits from `MultiTenantIdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
   - Add simple tenant-scoped entity (e.g., `PocArticle` with TenantId)
   - Mark entity with `[MultiTenant]` attribute
   - Expected outcome: DbContext compiles successfully
   - Files affected: `Task/PoC/FinbuckleMultitenantAuditNet/PocDbContext.cs`, `Task/PoC/FinbuckleMultitenantAuditNet/PocArticle.cs`
   - Reality check: Code builds without errors

4. **Configure Audit.NET data provider**
   - Override `SaveChangesAsync` in PocDbContext
   - Configure Audit.EntityFramework data provider (see references.md for pattern)
   - Add custom field to audit event to capture TenantId from `IMultiTenantContextAccessor`
   - Expected outcome: SaveChangesAsync calls both base implementation and Audit.NET
   - Files affected: `Task/PoC/FinbuckleMultitenantAuditNet/PocDbContext.cs`
   - Reality check: Code compiles, no runtime initialization errors

5. **Configure Finbuckle with StaticStrategy for POC**
   - Add Finbuckle services with `AddMultiTenant<TenantInfo>().WithStaticStrategy("test-tenant-id")`
   - Use StaticStrategy to provide predictable tenant for POC testing
   - Expected outcome: Tenant resolution works in POC
   - Files affected: `Task/PoC/FinbuckleMultitenantAuditNet/Program.cs` or POC setup
   - Reality check: IMultiTenantContextAccessor returns non-null TenantInfo

**Part 3: Testing & Validation**

6. **Create POC test**
   - Write xUnit test that:
     - Creates PocDbContext with tenant context
     - Inserts PocArticle entity
     - Calls SaveChangesAsync
     - Verifies entity is saved with correct TenantId
     - Queries entity back with query filter applied
     - Checks Audit.NET log for captured event with TenantId
   - Expected outcome: Test passes, demonstrating both concerns work together
   - Files affected: `Task/PoC/FinbuckleMultitenantAuditNet/PocTests.cs`
   - Reality check: Test passes green

7. **Validate query filters**
   - Add second test with different tenant context
   - Insert entity with TenantId "tenant-1"
   - Switch to TenantId "tenant-2" context
   - Query for all entities
   - Verify query returns empty (filter working)
   - Expected outcome: Query filters automatically restrict by TenantId
   - Reality check: Test passes, confirming isolation

8. **Inspect Audit.NET logs**
   - Manually inspect audit log output (console or file)
   - Verify audit events contain TenantId custom field
   - Confirm both EntityFrameworkEvent and DatabaseTransactionEvent include TenantId
   - Expected outcome: TenantId present in all audit events
   - Reality check: Logs show TenantId field with expected values

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After each code change
dotnet build Task/PoC/FinbuckleMultitenantAuditNet

# After adding tests
dotnet test Task/PoC/FinbuckleMultitenantAuditNet

# Inspect test output
cat Task/PoC/FinbuckleMultitenantAuditNet/test-output.log
```

Don't wait until the end to test. Reality test after each step.

### Expected Working State After Phase

When this phase is complete:
- POC demonstrates MultiTenantIdentityDbContext works with Audit.NET data provider
- Query filters automatically apply to tenant-scoped entities
- Audit.NET captures TenantId in audit events
- Both concerns (multi-tenancy and auditing) work together without conflicts
- Decision 1 from key-decisions.md is validated
- **Main codebase is unchanged** - all existing tests still pass
- POC code can be discarded or archived after validation

### If Phase Fails

If this phase fails and cannot be completed:
1. Document specific incompatibility found (e.g., SaveChangesAsync conflict, audit event missing TenantId)
2. Search docfork and mslearn for Finbuckle + Audit.NET integration patterns
3. Try debug-analysis.md to identify root cause
4. If POC demonstrates incompatibility, run `flowpilot stuck` to re-evaluate Decision 1 options (Option A or C)

### Verification

Run the following commands to verify this phase:

```bash
# POC must build
cd Task/PoC/FinbuckleMultitenantAuditNet
dotnet build

# POC tests must pass
dotnet test

# Verify audit logs contain TenantId
grep -r "TenantId" Task/PoC/FinbuckleMultitenantAuditNet/audit-output/
```

**Existing codebase verification (must still pass):**

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
```

All targets must pass. Since this is a POC phase, main codebase should be untouched.

**Manual Verification Steps:**
1. Review POC test results - both tests pass
2. Inspect audit log output - TenantId field present
3. Confirm query filter behavior - entities filtered by tenant
4. Document findings in POC summary (can add to references.md if patterns useful for future phases)