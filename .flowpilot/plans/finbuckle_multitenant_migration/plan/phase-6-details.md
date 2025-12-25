## phase_6: Create Custom Registration and Login Endpoints (Pure Refactoring)

### Phase Overview

Create custom `/api/identity/register` and `/api/identity/login` FastEndpoints that implement exactly the same behavior as current default ASP.NET Identity endpoints. This is a pure refactoring with no behavior changes, preparing for Organization creation and claims transformation in phase 7.

**Scope Size:** Small (~8 steps)
**Risk Level:** Medium (authentication is critical, but pure refactoring reduces risk)
**Estimated Complexity:** Medium

### Prerequisites

What must be completed before starting this phase:
- Phase 5 completed (TenantId added to entities, query filters active)
- All tests passing
- Understanding of current Identity authentication flow

### Known Risks & Mitigations

**Risk 1:** Custom endpoints may not replicate Identity behavior exactly
- **Likelihood:** Medium
- **Impact:** High (authentication broken)
- **Mitigation:** Use UserManager and SignInManager exactly as Identity endpoints do. Test thoroughly.
- **Fallback:** If custom endpoints don't work, revert to Identity endpoints, investigate differences

**Risk 2:** Existing tests should work without changes
- **Likelihood:** High
- **Impact:** Low (this is a pure refactoring)
- **Mitigation:** Since this is a pure refactoring with no URL, behavior, or payload changes, existing tests should work without modification
- **Fallback:** If tests fail, investigate differences in endpoint behavior

### Implementation Steps

**Part 1: Create Custom Register Endpoint**

1. **Create RegisterEndpoint class**
   - Create `App/Server/src/Server.Web/Endpoints/Identity/Register.cs`
   - Implement FastEndpoint with POST /api/identity/register route
   - Use AllowAnonymous (registration is public)
   - Expected outcome: Endpoint class structure ready
   - Files affected: `App/Server/src/Server.Web/Endpoints/Identity/Register.cs` (new)
   - Reality check: Code compiles

2. **Implement Register handler logic**
   - Inject UserManager<ApplicationUser>
   - Accept RegisterRequest DTO (email, username, password)
   - Call `UserManager.CreateAsync(user, password)`
   - Call `SignInManager.SignInAsync(user, isPersistent: false)`
   - Return user data (matching current registration response)
   - Expected outcome: Registration logic replicates Identity behavior
   - Files affected: `App/Server/src/Server.Web/Endpoints/Identity/Register.cs`
   - Reality check: Code compiles, follows existing patterns

3. **Create RegisterRequest and RegisterResponse DTOs**
   - Create request/response DTOs in `App/Server/src/Server.Web/Endpoints/Identity/`
   - Match existing registration contract (email, username, password)
   - Expected outcome: DTOs defined
   - Files affected: `App/Server/src/Server.Web/Endpoints/Identity/RegisterRequest.cs`, `RegisterResponse.cs`
   - Reality check: DTOs compile

**Part 2: Create Custom Login Endpoint**

4. **Create LoginEndpoint class**
   - Create `App/Server/src/Server.Web/Endpoints/Identity/Login.cs`
   - Implement FastEndpoint with POST /api/identity/login route
   - Use AllowAnonymous (login is public)
   - Expected outcome: Endpoint class structure ready
   - Files affected: `App/Server/src/Server.Web/Endpoints/Identity/Login.cs` (new)
   - Reality check: Code compiles

5. **Implement Login handler logic**
   - Inject UserManager<ApplicationUser> and SignInManager<ApplicationUser>
   - Accept LoginRequest DTO (email, password)
   - Call `SignInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false)`
   - Return user data if successful
   - Expected outcome: Login logic replicates Identity behavior
   - Files affected: `App/Server/src/Server.Web/Endpoints/Identity/Login.cs`
   - Reality check: Code compiles

6. **Create LoginRequest and LoginResponse DTOs**
   - Create request/response DTOs in `App/Server/src/Server.Web/Endpoints/Identity/`
   - Match existing login contract (email, password)
   - Expected outcome: DTOs defined
   - Files affected: `App/Server/src/Server.Web/Endpoints/Identity/LoginRequest.cs`, `LoginResponse.cs`
   - Reality check: DTOs compile

**Part 3: Update Identity Endpoint Configuration**

7. **Update Identity endpoint mapping configuration**
   - Find configuration that maps built-in Identity endpoints to `/api/identity` group
   - Update to use custom FastEndpoints instead of built-in Identity endpoints
   - Remove or disable the Identity endpoint mapping
   - Expected outcome: Custom endpoints replace built-in Identity endpoints at same URLs
   - Files affected: `App/Server/src/Server.Web/Program.cs` or endpoint configuration
   - Reality check: Application builds, endpoints registered correctly

8. **Run tests**
   - Run: `./build.sh TestServer`
   - Run: `./build.sh TestServerPostmanAuth`
   - Verify all authentication tests pass with custom endpoints
   - Expected outcome: Tests pass, authentication works
   - Reality check: Functional tests and Postman Auth tests pass

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After endpoint creation
./build.sh LintServerVerify
./build.sh BuildServer

# After test updates
./build.sh TestServer

# Full validation
./build.sh TestServerPostmanAuth
./build.sh TestE2e
```

Don't wait until the end to test. Reality test after each major change.

### Expected Working State After Phase

When this phase is complete:
- Custom /api/identity/register endpoint exists
- Custom /api/identity/login endpoint exists
- Both endpoints implement exact same behavior as Identity defaults at same URLs
- Existing tests pass without modification (pure refactoring)
- **No behavior changes** - pure refactoring
- Ready for phase 7 (add Organization creation and claims transformation)

### If Phase Fails

If this phase fails and cannot be completed:
1. Compare custom endpoint logic to ASP.NET Identity source code
2. Use mslearn MCP server to search for Identity UserManager and SignInManager patterns
3. Check that SignInManager.SignInAsync is called for cookie auth
4. Verify PasswordSignInAsync is used correctly
5. Use debug-analysis.md for authentication issues
6. If stuck, run `flowpilot stuck`

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostmanAuth
./build.sh TestE2e
```

All targets must pass before proceeding to the next phase.

**Manual Verification Steps:**
1. Start application: `./build.sh RunLocalPublish`
2. Test registration via Postman or frontend - should work identically to before
3. Test login via Postman or frontend - should work identically to before
4. Verify cookies are set correctly (check browser DevTools)
5. Verify no errors in logs
