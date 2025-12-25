## phase_7: Update Registration to Create Organization and Add TenantId Claim

### Phase Overview

Enhance the custom registration endpoint to create a new Organization when a user registers. Associate the user with their Organization (set TenantId). Assign "Owner" role. Implement IClaimsTransformation to add TenantId claim to user principal after authentication for tenant resolution in phase 8.

**Scope Size:** Small (~10 steps)
**Risk Level:** Medium (registration and auth flow changes are critical)
**Estimated Complexity:** Medium

### Prerequisites

What must be completed before starting this phase:
- Phase 6 completed (custom registration/login endpoints working)
- All tests passing with custom endpoints
- Understanding of IClaimsTransformation from references.md

### Known Risks & Mitigations

**Risk 1:** Organization creation may fail during registration
- **Likelihood:** Low
- **Impact:** High (user created but no organization)
- **Mitigation:** Use EF transaction scope, ensure Organization created before user. If creation fails, rollback user creation.
- **Fallback:** Add explicit transaction in registration handler

**Risk 2:** IClaimsTransformation may be called multiple times
- **Likelihood:** High (documented behavior)
- **Impact:** Low (performance concern)
- **Mitigation:** Check if TenantId claim already exists before adding. IClaimsTransformation should be idempotent.
- **Fallback:** Cache claims to avoid repeated database queries

### Implementation Steps

**Part 1: Update Registration Endpoint**

1. **Inject IRepository<Organization> into Register endpoint**
   - Update Register endpoint to inject IRepository<Organization>
   - Expected outcome: Repository available for Organization creation
   - Files affected: `App/Server/src/Server.Web/Endpoints/Identity/Register.cs`
   - Reality check: Code compiles

2. **Create Organization during registration**
   - Before creating ApplicationUser, create Organization entity
   - Set Organization.Name (use username or email prefix as default)
   - Set Organization.Identifier (use Guid or sanitized name)
   - Save Organization to database
   - Expected outcome: Organization created
   - Files affected: `App/Server/src/Server.Web/Endpoints/Identity/Register.cs`
   - Reality check: Registration creates Organization record

3. **Associate user with Organization**
   - Set ApplicationUser.TenantId to created Organization.Id
   - Expected outcome: User linked to Organization
   - Files affected: `App/Server/src/Server.Web/Endpoints/Identity/Register.cs`
   - Reality check: User has TenantId set

4. **Assign Owner role to user**
   - After user creation, call `UserManager.AddToRoleAsync(user, "Owner")`
   - Ensure "Owner" role exists (may need to seed in AppDbContext or migration)
   - Expected outcome: User has Owner role
   - Files affected: `App/Server/src/Server.Web/Endpoints/Identity/Register.cs`
   - Reality check: User in Owner role

**Part 2: Implement IClaimsTransformation**

5. **Create TenantClaimsTransformation class**
   - Create `App/Server/src/Server.Infrastructure/Identity/TenantClaimsTransformation.cs`
   - Implement IClaimsTransformation interface
   - Inject UserManager<ApplicationUser>
   - Expected outcome: Claims transformation class ready
   - Files affected: `App/Server/src/Server.Infrastructure/Identity/TenantClaimsTransformation.cs` (new)
   - Reality check: Code compiles

6. **Implement TransformAsync method**
   - In TransformAsync, check if TenantId claim already exists (return early if so)
   - Get user from ClaimsPrincipal: `var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)`
   - Query user with `UserManager.FindByIdAsync(userId)`
   - If user has TenantId, add claim: `identity.AddClaim(new Claim("TenantId", user.TenantId.ToString()))`
   - Return modified ClaimsPrincipal
   - Expected outcome: TenantId claim added to authenticated users
   - Files affected: `App/Server/src/Server.Infrastructure/Identity/TenantClaimsTransformation.cs`
   - Reality check: Claims transformation logic complete

7. **Register IClaimsTransformation in DI**
   - In Program.cs, register: `builder.Services.AddScoped<IClaimsTransformation, TenantClaimsTransformation>();`
   - Expected outcome: Claims transformation active
   - Files affected: `App/Server/src/Server.Web/Program.cs`
   - Reality check: Application starts without DI errors

**Part 3: Update Tests**

8. **Update registration tests**
   - Modify registration functional tests to verify Organization created
   - Verify user has TenantId set
   - Verify user has Owner role
   - Expected outcome: Registration tests validate new behavior
   - Files affected: `App/Server/tests/Server.FunctionalTests/Identity/RegistrationTests.cs`
   - Reality check: Tests pass

9. **Update login tests**
   - Verify TenantId claim is present after login
   - Check ClaimsPrincipal has TenantId claim
   - Expected outcome: Login tests validate claims transformation
   - Files affected: `App/Server/tests/Server.FunctionalTests/Identity/LoginTests.cs`
   - Reality check: Tests pass

10. **Update E2E tests**
    - E2E registration tests should verify user can register and login
    - After registration, organization exists and user belongs to it
    - Expected outcome: E2E tests validate end-to-end flow
    - Files affected: `Test/e2e/E2eTests/RegisterPage/*Tests.cs`
    - Reality check: E2E tests pass

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After registration updates
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer

# After claims transformation
./build.sh TestServer

# Full validation
./build.sh TestServerPostman
./build.sh TestE2e
```

Don't wait until the end to test. Reality test after each major change.

### Expected Working State After Phase

When this phase is complete:
- Registration creates new Organization
- User associated with Organization (TenantId set)
- User assigned Owner role for their Organization
- IClaimsTransformation adds TenantId claim after authentication
- All authenticated requests have TenantId claim in ClaimsPrincipal
- Tests pass with Organization creation logic
- **Tenant resolution not active yet** (phase 8 adds ClaimStrategy configuration)
- Ready for phase 8 (configure Finbuckle to resolve tenant from claim)

### If Phase Fails

If this phase fails and cannot be completed:
1. If Organization creation fails, check transaction semantics in EF Core
2. Use mslearn MCP server to search for IClaimsTransformation examples
3. Verify claims transformation is called by inspecting ClaimsPrincipal in debugger
4. Check that TenantId claim name matches ("TenantId" exactly)
5. Use debug-analysis.md for complex auth issues
6. If stuck, run `flowpilot stuck`

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
1. Start application: `./build.sh RunLocalPublish`
2. Register new user via frontend
3. Check database - verify Organization created and user has TenantId
4. Check database - verify user has Owner role
5. Login and inspect ClaimsPrincipal (add logging or debugger breakpoint)
6. Verify TenantId claim present in principal
