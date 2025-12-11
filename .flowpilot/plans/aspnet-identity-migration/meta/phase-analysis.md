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

**Goal**: Add Identity API endpoints and update authentication middleware

**Key Outcomes**:
- Add MapIdentityApi<ApplicationUser>() to expose Identity endpoints (/register, /login, /refresh, etc.)
- Remove old authentication endpoints (Users/Register, Users/Login)
- Update middleware pipeline for cookie authentication (remove JWT authentication)
- Remove old authentication services (IJwtTokenGenerator, IPasswordHasher, BcryptPasswordHasher, JwtTokenGenerator)
- Application serves both old and new endpoints during transition

**Working State Transition**: Identity endpoints are now available and functional. Old JWT-based endpoints are removed. Cookie authentication replaces JWT authentication. Application builds and runs but tests need updating to use new endpoints and cookie-based auth.

---

### phase_4

**Goal**: Update functional tests to use Identity endpoints and cookie authentication

**Key Outcomes**:
- Update UsersTests to use Identity endpoints (/register, /login instead of /api/users, /api/users/login)
- Update test helpers to use cookie-based authentication instead of JWT tokens
- Update ArticlesFixture and other test fixtures to authenticate via Identity
- Refactor test HttpClient creation to preserve cookies across requests
- All functional tests pass with new authentication system

**Working State Transition**: Functional test suite is fully updated and passing with Identity endpoints. Backend integration tests validate the new authentication flows work correctly. Application is functionally complete but Postman and E2E tests still need updating.

---

### phase_5

**Goal**: Update Postman collection to use Identity endpoints

**Key Outcomes**:
- Update Postman auth requests to use /register and /login endpoints
- Update Postman environment to use cookies instead of JWT tokens
- Configure Postman to send/receive cookies automatically
- Update pre-request scripts if needed for cookie handling
- All Postman tests pass with Identity endpoints

**Working State Transition**: Postman test suite fully validates the Identity endpoints. API contract is confirmed working end-to-end through Postman. E2E tests remain the final validation step.

---

### phase_6

**Goal**: Update E2E Playwright tests and frontend to use cookie authentication

**Key Outcomes**:
- Update frontend API client to use Identity endpoints (/register, /login)
- Remove JWT token handling from frontend (Authorization: Token header)
- Update frontend to rely on automatic cookie handling by browser
- Update Playwright E2E tests for new authentication flows
- All E2E tests pass with cookie-based authentication

**Working State Transition**: Complete end-to-end system works with ASP.NET Identity. Frontend communicates with Identity endpoints, cookies are managed automatically by browser, and all E2E tests validate the full stack. Migration is functionally complete.

---

### phase_7

**Goal**: Clean up legacy code and update documentation

**Key Outcomes**:
- Remove unused User entity and related specifications (UserByEmailAndPasswordSpec, etc.)
- Remove unused authentication-related services and interfaces
- Remove old authentication configuration code
- Remove JWT-related NuGet packages (JwtBearer, IdentityModel.Tokens.Jwt, BCrypt.Net-Next)
- Update API documentation to reflect Identity endpoints
- Clean up any remaining TODO comments or temporary code

**Working State Transition**: Codebase is clean with all legacy authentication code removed. Only Identity-based authentication code remains. All tests pass. Application is production-ready with fully migrated authentication system.

---