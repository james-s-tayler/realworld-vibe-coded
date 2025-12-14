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

**Goal**: Update Postman collection to use Identity endpoints

**Key Outcomes**:
- Update Postman auth requests to use /api/identity/register and /api/identity/login endpoints
- Update Postman environment to use cookies instead of JWT tokens
- Configure Postman to send/receive cookies automatically
- Update pre-request scripts if needed for cookie handling
- All Postman tests pass with Identity endpoints
- old /api/users/register and /api/users/login endpoints remain operational, since they are still called by the frontend, and needed for the e2e tests to pass at this point, but the postman collection no longer uses them at all.

**Working State Transition**: Postman test suite fully validates the Identity endpoints. API contract is confirmed working end-to-end through Postman. Dual operation continues - both auth systems still work. E2E tests still use old endpoints and continue to pass.

---

### phase_6

**Goal**: Update E2E Playwright tests and frontend to use cookie authentication

**Key Outcomes**:
- Update frontend API client to use Identity endpoints (/register, /login)
- Remove JWT token handling from frontend (Authorization: Token header)
- Update frontend to rely on automatic cookie handling by browser
- Update Playwright E2E tests for new authentication flows
- All E2E tests pass with cookie-based authentication
- Old JWT-based endpoints remain operational (unused but available)

**Working State Transition**: Complete end-to-end system works with ASP.NET Identity. Frontend communicates with Identity endpoints, cookies are managed automatically by browser, and all E2E tests validate the full stack. All tests now use Identity. Old JWT system is still present but no longer used.

---

### phase_7

**Goal**: Remove legacy JWT authentication system

**Key Outcomes**:
- Remove old authentication endpoints (Users/Register, Users/Login)
- Remove old authentication services (IJwtTokenGenerator, IPasswordHasher, BcryptPasswordHasher, JwtTokenGenerator)
- Remove JWT authentication from middleware pipeline
- Remove unused User entity and related specifications (UserByEmailAndPasswordSpec, etc.)
- Remove JWT-related NuGet packages (JwtBearer, IdentityModel.Tokens.Jwt, BCrypt.Net-Next)
- Clean up any remaining TODO comments or temporary code related to old auth

**Working State Transition**: Codebase is clean with all legacy authentication code removed. Only Identity-based authentication code remains. All tests pass using Identity endpoints. Application is production-ready with fully migrated authentication system.

---

### phase_8

**Goal**: Final cleanup and documentation

**Key Outcomes**:
- Update API documentation to reflect Identity endpoints only
- Review and clean up any migration-related comments or documentation
- Verify all tests pass (functional, Postman, E2E)
- Verify all Nuke build targets pass (LintAllVerify, BuildServer, TestServer, TestServerPostman, TestE2e)
- Final code review and validation

**Working State Transition**: Migration is complete. Documentation is up-to-date. All tests and build targets pass. System is production-ready with clean, maintainable code using ASP.NET Identity exclusively.

---