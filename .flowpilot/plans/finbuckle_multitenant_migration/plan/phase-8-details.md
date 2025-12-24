## phase_8: Implement Tenant Resolution via ClaimStrategy

### Phase Overview

Configure Finbuckle.MultiTenant with ClaimStrategy to resolve current tenant from TenantId claim. Create custom TenantInfo class mapping to Organization entity. Configure middleware ordering (UseMultiTenant before UseAuthentication). Update functional tests to use StaticStrategy for known tenant context. Query filters now work end-to-end.

**Scope Size:** Small (~8 steps)
**Risk Level:** High (middleware ordering is critical, tenant resolution is core functionality)
**Estimated Complexity:** Medium

### Prerequisites

What must be completed before starting this phase:
- Phase 7 completed (TenantId claim added to authenticated users)
- All tests passing with Organization creation and claims transformation
- Understanding of middleware ordering from references.md (critical!)

### Known Risks & Mitigations

**Risk 1:** Middleware ordering incorrect - tenant resolution won't work
- **Likelihood:** Medium
- **Impact:** Critical (multi-tenancy completely broken)
- **Mitigation:** Follow references.md exactly: UseMultiTenant() MUST come BEFORE UseAuthentication(). Test immediately after configuration.
- **Fallback:** Review middleware order, check Finbuckle docs, use mslearn MCP server

**Risk 2:** ClaimStrategy may not find TenantId claim
- **Likelihood:** Low (phase 7 added claim)
- **Impact:** High (tenant not resolved)
- **Mitigation:** Verify claim name matches exactly ("TenantId"). Log IMultiTenantContext to verify resolution.
- **Fallback:** Add logging to IClaimsTransformation and ClaimStrategy to debug

**Risk 3:** EF Core store may not resolve tenant from Organization table
- **Likelihood:** Low
- **Impact:** High (tenant resolution fails)
- **Mitigation:** Configure EF Core store correctly to query Organizations by Identifier
- **Fallback:** Use InMemoryStore temporarily for testing, then fix EF Core store

### Implementation Steps

**Part 1: Create Custom TenantInfo Class**

1. **Create TenantInfo class**
   - Create `App/Server/src/Server.Infrastructure/MultiTenancy/AppTenantInfo.cs`
   - Inherit from `TenantInfo` (Finbuckle base class)
   - Add additional properties if needed (or keep simple with just Id, Identifier, Name)
   - Map to Organization entity properties
   - Expected outcome: Custom TenantInfo ready
   - Files affected: `App/Server/src/Server.Infrastructure/MultiTenancy/AppTenantInfo.cs` (new)
   - Reality check: Code compiles

**Part 2: Configure Finbuckle Services**

2. **Add Finbuckle services in Program.cs**
   - In Program.cs, add: `builder.Services.AddMultiTenant<AppTenantInfo>().WithClaimStrategy("TenantId").WithEFCoreStore<AppDbContext, AppTenantInfo>();`
   - WithClaimStrategy("TenantId") tells Finbuckle to look for TenantId claim
   - WithEFCoreStore uses AppDbContext to query Organizations table
   - Expected outcome: Finbuckle services registered
   - Files affected: `App/Server/src/Server.Web/Program.cs`
   - Reality check: Application starts without DI errors

3. **Configure middleware ordering**
   - In Program.cs, ensure: `app.UseMultiTenant();` comes BEFORE `app.UseAuthentication();`
   - This is CRITICAL - ClaimStrategy needs claims which come from authentication
   - Correct order: UseMultiTenant → UseAuthentication → UseAuthorization
   - Expected outcome: Middleware ordered correctly
   - Files affected: `App/Server/src/Server.Web/Program.cs`
   - Reality check: Application starts, middleware order correct

**Part 3: Update AppDbContext for Finbuckle Store**

4. **Ensure Organization entity works as tenant store**
   - AppDbContext already inherits from MultiTenantIdentityDbContext
   - Verify Organization entity has Identifier property (required by Finbuckle)
   - EF Core store will query Organizations by Identifier to resolve tenant
   - Expected outcome: EF Core store can query Organizations
   - Files affected: No changes (verification only)
   - Reality check: Organization.Identifier exists and is unique

**Part 4: Update Functional Tests**

5. **Configure StaticStrategy for tests**
   - Update WebApplicationFactory configuration in test fixtures
   - Override Finbuckle services to use StaticStrategy instead of ClaimStrategy
   - Use known tenant ID for tests (e.g., test Organization's Identifier)
   - Expected outcome: Tests have predictable tenant context
   - Files affected: `App/Server/tests/Server.FunctionalTests/Support/AppFixture.cs` or similar
   - Reality check: Test setup compiles

6. **Update test fixtures to create tenant context**
   - Ensure test fixtures create Organization and set tenant context via StaticStrategy
   - Tests should use: `.WithStaticStrategy([tenantIdentifier])`
   - Expected outcome: Tests have tenant context
   - Files affected: `App/Server/tests/Server.FunctionalTests/Fixtures/*.cs`
   - Reality check: Tests set up tenant correctly

**Part 5: Test Tenant Resolution**

7. **Add tenant resolution verification**
   - Create test that verifies IMultiTenantContextAccessor<AppTenantInfo> returns non-null after authentication
   - Verify TenantInfo.Id matches user's Organization
   - Expected outcome: Tenant resolved correctly
   - Files affected: `App/Server/tests/Server.FunctionalTests/MultiTenancy/TenantResolutionTests.cs` (new)
   - Reality check: Tenant resolution test passes

8. **Run all tests**
   - Run: `./build.sh TestServer`
   - Fix any tests failing due to tenant filtering
   - Expected outcome: All functional tests pass
   - Reality check: 45 functional tests pass

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After TenantInfo creation
./build.sh BuildServer

# After Finbuckle configuration
./build.sh BuildServer
# Application should start

# After test updates
./build.sh TestServer

# Full validation
./build.sh TestServerPostman
./build.sh TestE2e
```

Don't wait until the end to test. Reality test after each major change.

### Expected Working State After Phase

When this phase is complete:
- Finbuckle.MultiTenant configured with ClaimStrategy
- Middleware ordered correctly (UseMultiTenant before UseAuthentication)
- EF Core store configured to query Organizations
- Tenant resolved from TenantId claim after authentication
- IMultiTenantContextAccessor provides current tenant context
- Query filters work end-to-end (tenant resolved → filters applied automatically)
- Functional tests use StaticStrategy for predictable tenant context
- **Data isolation enforced on reads** (queries automatically filtered by TenantId)
- Ready for phase 9 (set TenantId on entity creation)

### If Phase Fails

If this phase fails and cannot be completed:
1. FIRST: Check middleware ordering - UseMultiTenant MUST be before UseAuthentication
2. Check that TenantId claim name matches exactly in IClaimsTransformation and WithClaimStrategy
3. Add logging to verify tenant resolution: log IMultiTenantContext in middleware
4. Use mslearn MCP server to search for Finbuckle ClaimStrategy examples
5. Verify Organization.Identifier is set correctly in registration
6. Check EF Core store configuration - ensure Organizations table is queryable
7. Use debug-analysis.md for complex tenant resolution issues
8. If stuck, run `flowpilot stuck`

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass before proceeding to the next phase.

**Manual Verification Steps:**
1. Start application: `./build.sh RunLocal`
2. Register new user, login
3. Add logging in a handler to check IMultiTenantContextAccessor - verify TenantInfo not null
4. Check that TenantInfo.Id matches user's Organization.Id
5. Verify queries for Articles return only articles with matching TenantId (create test articles with different TenantIds)
6. Check Finbuckle middleware logs for tenant resolution
