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

### phase_1

**Goal**: Add ASP.NET Identity infrastructure and create ApplicationUser entity

**Key Outcomes**:
- Install required NuGet packages (Microsoft.AspNetCore.Identity.EntityFrameworkCore, Audit.EntityFramework.Identity)
- Create ApplicationUser class extending IdentityUser<Guid> with custom properties (Bio, Image) and Following/Followers relationships
- Update AppDbContext to inherit from AuditIdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
- Create and apply EF Core migration to add Identity tables
- Application still builds and existing tests pass (no breaking changes yet)

**Working State Transition**: The application remains functional with old authentication system intact. Identity infrastructure is added alongside existing auth, not replacing it. Database schema includes both old User table and new AspNetUsers tables. All existing tests continue to pass.

---

### phase_2

**Goal**: Configure ASP.NET Identity services and cookie authentication

**Key Outcomes**:
- Configure Identity services with AddIdentity<ApplicationUser, IdentityRole<Guid>>()
- Configure password policy (MinLength=8, balanced security settings per Decision 5)
- Configure cookie authentication with SameSite=Lax
- Configure lockout policy (5 attempts, 10 minute duration)
- Update IUserContext implementation to work with Identity's cookie-based claims
- Identity services configured but MapIdentityApi not yet added

**Working State Transition**: Identity services are registered and configured but not yet mapped to endpoints. Old authentication endpoints still work. Application builds and starts successfully with both auth systems configured side-by-side. Tests continue to pass using old endpoints.

---

### phase_3

**Goal**: Add Identity API endpoints alongside existing authentication (dual operation)

**Key Outcomes**:
- Add MapIdentityApi<ApplicationUser>() to expose Identity endpoints (/register, /login, /refresh, etc.)
- Configure middleware pipeline to support BOTH cookie authentication (Identity) AND JWT authentication (existing)
- Keep all existing authentication endpoints (Users/Register, Users/Login) functional
- Keep all existing authentication services (IJwtTokenGenerator, IPasswordHasher, etc.) operational
- Both authentication systems work simultaneously and independently

**Working State Transition**: Application now supports dual authentication - both Identity cookie-based endpoints AND legacy JWT-based endpoints work side-by-side. All existing tests (functional, Postman, E2E) continue to pass using the old JWT endpoints. New Identity endpoints are available and functional for parallel testing. No breaking changes.

---

### phase_4

**Goal**: Complete migration to ApplicationUser as single source of truth (Option A approach)

**Key Outcomes**:
- ALL handlers migrated from IRepository<User> to UserManager<ApplicationUser>
- Register, Login, GetCurrent, UpdateUser use Identity UserManager/SignInManager
- Article, Profile, Comment handlers updated to use ApplicationUser
- JWT token generation updated to work with ApplicationUser
- Legacy User entity and IRepository<User> completely removed
- Only AspNetUsers table remains (Users table dropped)
- All tests updated and passing with ApplicationUser
- FastEndpoints support both cookie and Token authentication

**Working State Transition**: Complete migration from legacy User entity to ApplicationUser. All user data stored exclusively in AspNetUsers table. All handlers query/update via UserManager. Legacy User table dropped. JWT tokens generated from ApplicationUser. Cross-authentication works (register via /api/users or /api/identity, login via either). All tests pass.

---

### phase_5

**Goal**: Split monolithic Postman collection into individual collections

**Key Outcomes**:
- Postman collection split into separate collections: Auth, Profiles, Feed, Articles
- Nuke targets added to run individual collections independently
- Each collection can be tested in isolation
- All collections continue to pass with existing JWT authentication

**Working State Transition**: Postman test suite is now modular and easier to reason about. Each collection can be updated independently. All collections still use JWT authentication and continue to pass. No breaking changes to backend.

---

### phase_6

**Goal**: Make username optional on /api/users/register and update Auth postman collection

**Key Outcomes**:
- Username parameter made optional on /api/users/register endpoint
- When username not provided, it defaults to email value
- Auth postman collection updated to remove username from test data
- Auth postman tests pass without providing username
- Other postman collections still pass (they may or may not provide username)

**Working State Transition**: Register endpoint now supports both old usage (with username) and new usage (without username, defaults to email). Auth collection updated and passing. Other collections unchanged. Frontend and E2E tests unchanged. Migration pathway established for incremental updates.

---

### phase_7

**Goal**: Update Profiles postman collection to remove username dependency

**Key Outcomes**:
- Profiles collection updated to not pass username when registering test users
- Test setup data updated to remove username fields
- Profiles collection tests pass using email as username
- Other collections unchanged

**Working State Transition**: Auth and Profiles collections now use email-based approach. Articles and Feed collections still use old approach. Frontend and E2E unchanged. System continues to work with both approaches.

---

### phase_8

**Goal**: Update Feed postman collection to remove username dependency

**Key Outcomes**:
- Feed collection updated to not pass username when registering test users
- Test setup data updated to remove username fields
- Feed collection tests pass using email as username
- Other collections unchanged

**Working State Transition**: Auth, Profiles, and Feed collections now use email-based approach. Articles collection still uses old approach. Frontend and E2E unchanged. System continues to work with both approaches.

---

### phase_9

**Goal**: Update Articles postman collection to remove username dependency

**Key Outcomes**:
- Articles collection updated to not pass username when registering test users
- Test setup data updated to remove username fields
- Articles collection tests pass using email as username
- All postman collections now use email as username

**Working State Transition**: All postman collections now use email-based approach for username. Frontend and E2E tests still use old approach with explicit username. System continues to work with both approaches.

---

### phase_10

**Goal**: Update frontend and E2E tests to remove username from registration

**Key Outcomes**:
- Frontend register form updated to remove username field
- Frontend API calls updated to not send username to /api/users/register
- E2E tests updated to not provide username when registering
- All frontend tests and E2E tests pass
- All postman tests continue to pass

**Working State Transition**: Frontend and all tests now use email as username. No code explicitly provides username anymore. System is simpler and more consistent. All tests pass. Next phase can safely change token handling.

---

### phase_11

**Goal**: Update frontend and E2E tests to use explicit login after register

**Key Outcomes**:
- Frontend updated to ignore token in /api/users/register response
- Frontend updated to make explicit call to /api/users/login after successful register
- E2E tests updated to use explicit login after register
- All frontend tests and E2E tests pass
- Postman collections unchanged (still use token from register response)

**Working State Transition**: Frontend and E2E tests now follow two-step flow: register then login. This matches Identity's pattern where register doesn't return a token. Postman collections still use old single-step pattern. Backend supports both patterns.

---

### phase_12

**Goal**: Update postman collections to use explicit login after register

**Key Outcomes**:
- All postman collections updated to ignore token from /api/users/register response
- All collections updated to make explicit /api/users/login call after register
- All postman collections pass with two-step authentication flow
- Frontend and E2E already using two-step flow from phase 11

**Working State Transition**: All tests (frontend, E2E, postman) now use two-step authentication: register then login. This prepares for switching to Identity endpoints which don't return tokens on register. Backend still uses JWT but pattern now matches Identity's approach.

---

### phase_13

**Goal**: Support Identity bearer token scheme and update Auth postman collection

**Key Outcomes**:
- Backend configured to support three authentication schemes: "Token" (JWT), Identity Bearer, Identity Cookie
- Auth postman collection switched to use /api/identity/register and /api/identity/login
- Auth collection uses Identity bearer token scheme (Authorization: Bearer <token>)
- Auth collection tests pass with Identity endpoints
- Other collections still use /api/users endpoints with "Token" scheme
- Frontend and E2E still use /api/users endpoints

**Working State Transition**: Backend supports three auth schemes simultaneously. Auth collection validates Identity endpoints work. Other tests unchanged. Dual authentication fully operational. Migration pathway clear for remaining collections.

---

### phase_14

**Goal**: Update remaining postman collections to Identity endpoints

**Key Outcomes**:
- Profiles, Feed, and Articles collections switched to /api/identity/register and /api/identity/login
- All postman collections use Identity bearer token scheme
- All postman collections pass with Identity endpoints
- Frontend and E2E still use /api/users endpoints
- Old /api/users endpoints remain functional

**Working State Transition**: All postman collections now validate Identity endpoints. API contract proven end-to-end. Frontend and E2E still use old endpoints. Dual authentication continues. Ready to migrate frontend.

---

### phase_15

**Goal**: Update frontend and E2E tests to use Identity endpoints

**Key Outcomes**:
- Frontend updated to call /api/identity/register and /api/identity/login
- Frontend uses Identity bearer token scheme (Authorization: Bearer <token>)
- E2E tests updated for Identity endpoints
- All frontend tests and E2E tests pass
- Old /api/users endpoints still functional but unused

**Working State Transition**: Entire system (frontend, backend, all tests) now uses Identity endpoints with bearer tokens. Old JWT endpoints remain but are unused. Ready to add cookie support for production use.

---

### phase_16

**Goal**: Add CSRF protection and switch to cookie authentication

**Key Outcomes**:
- Backend configured to support CSRF protection
- Frontend updated to use cookie authentication via /api/identity/login?useCookies=true
- Add /api/identity/logout endpoint using SignInManager
- Frontend and E2E tests updated to use cookie authentication
- All tests pass with cookie-based authentication
- Bearer token authentication still works but unused

**Working State Transition**: Production-ready authentication with cookies and CSRF protection. Frontend uses secure cookie-based auth. All tests pass. Old JWT endpoints unused but present. Ready for final cleanup.

---

### phase_17

**Goal**: Decommission legacy /api/users/register and /api/users/login endpoints

**Key Outcomes**:
- Remove /api/users/register endpoint and handler
- Remove /api/users/login endpoint and handler
- Remove JWT token generation services (IJwtTokenGenerator, JwtTokenGenerator)
- Remove JWT authentication scheme configuration
- Remove JWT-related NuGet packages
- All tests pass with Identity endpoints only

**Working State Transition**: Migration complete. Only Identity endpoints remain. Codebase is clean with no legacy authentication code. All tests pass. System is production-ready with ASP.NET Identity exclusively.

---