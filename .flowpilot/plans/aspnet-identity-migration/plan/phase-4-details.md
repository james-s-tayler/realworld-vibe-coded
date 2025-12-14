## phase_4

### Phase Overview

Build and test a sync mechanism between the existing Users table and the new ASP.NET Identity AspNetUsers table, and enable cookie authentication for all FastEndpoints endpoints. This phase establishes bidirectional synchronization ensuring users can authenticate through either system regardless of where they registered, and proves this functionality through comprehensive functional tests.

### Prerequisites

- Phase 3 completed: Identity API endpoints available via MapIdentityApi at /api/identity
- Both authentication systems working independently
- Manual testing confirms Identity endpoints work correctly

### Implementation Steps

1. **Create User Synchronization Service**
   - Create `Server.Infrastructure/Identity/UserSyncService.cs` implementing `IUserSyncService`
   - Implement method `SyncLegacyUserToIdentity(User legacyUser)`:
     - Takes a legacy User entity and creates/updates corresponding ApplicationUser
     - Maps Username -> UserName, Email -> Email
     - Maps Password hash from legacy system to Identity's PasswordHash
     - Maps Bio and Image custom properties
     - Handle duplicate detection (by email or username)
   - Implement method `SyncIdentityUserToLegacy(ApplicationUser identityUser)`:
     - Takes an ApplicationUser and creates/updates corresponding legacy User entity
     - Maps UserName -> Username, Email -> Email
     - Maps PasswordHash to legacy password hash format
     - Maps Bio and Image custom properties
     - Handle duplicate detection

2. **Integrate Sync Service into Registration Flows**
   - Update `Server.Web/Users/Register/RegisterHandler.cs`:
     - After creating legacy User, call `UserSyncService.SyncLegacyUserToIdentity()`
     - Ensure ApplicationUser is created in AspNetUsers table
   - Create event handler or middleware for Identity registration:
     - Intercept Identity `/api/identity/register` endpoint completion
     - Call `UserSyncService.SyncIdentityUserToLegacy()`
     - Ensure legacy User is created in Users table
   - Both registration paths should result in entries in both tables

3. **Integrate Sync Service into Login Flows**
   - Update `Server.Web/Users/Login/LoginHandler.cs`:
     - After successful legacy login, verify ApplicationUser exists
     - If missing, sync legacy User to Identity using `SyncLegacyUserToIdentity()`
   - For Identity login, no handler modification needed (ASP.NET Identity handles it)
   - Both login paths should ensure both user records exist

4. **Enable Cookie Authentication for FastEndpoints**
   - Open `Server.Web/Configurations/AuthConfig.cs` (or wherever auth is configured)
   - Update FastEndpoints configuration to accept both authentication schemes:
     - Default scheme: Cookie (for Identity)
     - Additional scheme: "Token" (for JWT)
   - Update all FastEndpoints endpoints to accept both schemes:
     ```csharp
     AuthSchemes("Token", CookieAuthenticationDefaults.AuthenticationScheme)
     ```
   - This allows endpoints to work with either JWT tokens or cookies
   - Verify IUserContext implementation can extract user from both auth types

5. **Create Functional Tests for Cross-Authentication (Legacy -> Identity)**
   - Create `Server.FunctionalTests/Users/UserSyncTests.cs` test class
   - Test: Register via `/api/users/register`, then login via `/api/identity/login`:
     ```csharp
     // Use raw HttpClient for Identity endpoints with SRV007 suppression
     // SRV007: Using raw HttpClient.PostAsJsonAsync is necessary to test ASP.NET Identity
     // endpoints which are not FastEndpoints and return different response structures
     #pragma warning disable SRV007
     var identityLoginResponse = await client.PostAsJsonAsync("/api/identity/login", new { ... });
     #pragma warning restore SRV007
     ```
   - Verify login succeeds and cookie is set
   - Test: Register via `/api/users/register`, call `/api/user` with Token auth:
     ```csharp
     // Use FastEndpoints POSTAsync for legacy registration
     var (regResponse, regResult) = await client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request);
     // Extract token and set Authorization header
     client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", regResult.User.Token);
     // Call /api/user
     var (userResponse, userResult) = await client.GETAsync<GetCurrent, GetCurrentResponse>();
     ```
   - Verify user data is retrieved correctly

6. **Create Functional Tests for Cross-Authentication (Identity -> Legacy)**
   - In `UserSyncTests.cs`, add tests for opposite direction
   - Test: Register via `/api/identity/register`, then login via `/api/users/login`:
     ```csharp
     // Register with Identity (raw HttpClient)
     #pragma warning disable SRV007
     var identityRegResponse = await client.PostAsJsonAsync("/api/identity/register", new { ... });
     #pragma warning restore SRV007
     
     // Login via legacy endpoint (FastEndpoints)
     var loginRequest = new LoginRequest { ... };
     var (loginResponse, loginResult) = await client.POSTAsync<Login, LoginRequest, LoginResponse>(loginRequest);
     ```
   - Verify login succeeds and JWT token is returned
   - Test: Register via `/api/identity/register`, call `/api/user` with cookie auth:
     ```csharp
     // Register with Identity (sets cookie automatically)
     #pragma warning disable SRV007
     var identityRegResponse = await client.PostAsJsonAsync("/api/identity/register", new { ... });
     #pragma warning restore SRV007
     
     // Cookie is automatically sent with subsequent requests
     var (userResponse, userResult) = await client.GETAsync<GetCurrent, GetCurrentResponse>();
     ```
   - Verify user data is retrieved correctly via cookie auth

7. **Update Test Fixtures for Cookie Support**
   - Update `UsersFixture.cs` and other test fixtures:
     ```csharp
     protected HttpClient CreateClientWithCookieSupport()
     {
         return CreateClient(new WebApplicationFactoryClientOptions
         {
             AllowAutoRedirect = false,
             HandleCookies = true
         });
     }
     ```
   - Add helper methods for both auth types:
     - `CreateClientWithJwtAuth(string email, string password)` - returns client with JWT token
     - `CreateClientWithCookieAuth(string email, string password)` - returns client with cookie

8. **Handle Password Hash Compatibility**
   - Legacy system uses BCrypt password hashing
   - ASP.NET Identity uses its own password hasher
   - Options:
     a) Store original hash format marker and use appropriate hasher for login
     b) Re-hash passwords during sync (user must login to complete sync)
     c) Use custom IPasswordHasher<ApplicationUser> that supports both formats
   - Document decision and implement chosen approach
   - Ensure tests verify password validation works after sync

9. **Run and Fix Tests**
   - Run `./build.sh TestServer` to execute all functional tests
   - Fix any issues with:
     - Sync logic (missing fields, incorrect mappings)
     - Cookie handling in tests
     - Password hash compatibility
     - Database transaction boundaries
   - Ensure no regressions in existing tests

10. **Verify Dual Authentication Works**
    - Manually test the complete flows:
      - Register via legacy -> login via Identity
      - Register via Identity -> login via legacy
      - Access protected endpoints with both auth types
    - Verify both tables (Users and AspNetUsers) are in sync
    - Verify no data loss or corruption during sync

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh TestServer
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass. The sync mechanism should ensure users can authenticate through either system. Functional tests should validate all cross-authentication scenarios. Postman and E2E tests continue to use their existing authentication methods and pass without changes.