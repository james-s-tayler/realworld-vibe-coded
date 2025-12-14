## phase_4

### Phase Overview

Migrate the existing /api/users/register and /api/users/login endpoints to use ASP.NET Identity's UserManager and SignInManager internally, and enable cookie authentication for all FastEndpoints endpoints. This phase transitions the legacy endpoints to use Identity infrastructure while maintaining their existing API contracts, ensuring backward compatibility while leveraging Identity's robust authentication features.

### Prerequisites

- Phase 3 completed: Identity API endpoints available via MapIdentityApi at /api/identity
- Both authentication systems working independently
- Manual testing confirms Identity endpoints work correctly

### Implementation Steps

1. **Update Register Endpoint to Use UserManager**
   - Open `Server.Web/Users/Register/RegisterHandler.cs`
   - Inject `UserManager<ApplicationUser>` into the handler
   - Replace current user creation logic with UserManager:
     ```csharp
     // Create ApplicationUser from request
     var user = new ApplicationUser
     {
         UserName = request.User.Username,
         Email = request.User.Email,
         Bio = string.Empty,  // Default empty, can be updated later
         Image = null
     };
     
     // Use UserManager to create user with password
     var result = await _userManager.CreateAsync(user, request.User.Password);
     
     if (!result.Succeeded)
     {
         // Handle validation errors from Identity
         // Map IdentityError to response format
     }
     ```
   - Remove legacy User entity creation code
   - Remove calls to legacy password hasher
   - Keep JWT token generation for backward compatibility (existing clients expect token in response)
   - Return same response format (UserResponse with token)

2. **Update Login Endpoint to Use SignInManager**
   - Open `Server.Web/Users/Login/LoginHandler.cs`
   - Inject `SignInManager<ApplicationUser>` and `UserManager<ApplicationUser>`
   - Replace current authentication logic with SignInManager:
     ```csharp
     // Find user by email
     var user = await _userManager.FindByEmailAsync(request.User.Email);
     
     if (user == null)
     {
         // Return authentication failed
     }
     
     // Verify password using SignInManager
     var result = await _signInManager.CheckPasswordSignInAsync(user, request.User.Password, lockoutOnFailure: false);
     
     if (!result.Succeeded)
     {
         // Return authentication failed
     }
     ```
   - Remove legacy password verification code
   - Keep JWT token generation for backward compatibility
   - Return same response format (LoginResponse with token)

3. **Update GetCurrent Endpoint for Identity Users**
   - Open `Server.Web/Users/GetCurrent/GetCurrentHandler.cs`
   - Update `IUserContext` implementation to work with both auth schemes
   - If user authenticated via cookie (Identity), look up ApplicationUser
   - If user authenticated via JWT token, continue using existing logic
   - Map ApplicationUser to UserDto response format
   - Ensure Bio and Image are retrieved from ApplicationUser

4. **Update Update User Endpoint for Identity Users**
   - Open `Server.Web/Users/Update/UpdateHandler.cs`
   - Inject `UserManager<ApplicationUser>`
   - Update user information logic to work with ApplicationUser:
     ```csharp
     var user = await _userManager.FindByIdAsync(userId);
     
     // Update properties
     user.Email = request.User.Email ?? user.Email;
     user.UserName = request.User.Username ?? user.UserName;
     user.Bio = request.User.Bio ?? user.Bio;
     user.Image = request.User.Image ?? user.Image;
     
     // Update password if provided
     if (!string.IsNullOrEmpty(request.User.Password))
     {
         var token = await _userManager.GeneratePasswordResetTokenAsync(user);
         await _userManager.ResetPasswordAsync(user, token, request.User.Password);
     }
     
     var result = await _userManager.UpdateAsync(user);
     ```
   - Remove legacy User entity update code

5. **Enable Cookie Authentication for FastEndpoints**
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

6. **Update IUserContext Implementation**
   - Open `Server.Infrastructure/Identity/UserContext.cs` (or similar)
   - Update to work with both ApplicationUser and legacy User
   - Check authentication scheme to determine which user store to query
   - For cookie auth: query ApplicationUser by ClaimsPrincipal user ID
   - For JWT auth: continue using existing logic
   - Ensure consistent user information is returned regardless of auth method

7. **Create Functional Tests for Cross-Authentication (Legacy Endpoint -> Identity Login)**
   - Create `Server.FunctionalTests/Users/CrossAuthenticationTests.cs` test class
   - Test: Register via `/api/users/register`, then login via `/api/identity/login`:
     ```csharp
     // Register via legacy endpoint (which now uses UserManager internally)
     var registerRequest = new RegisterRequest
     {
         User = new UserData
         {
             Email = "test@example.com",
             Username = "testuser",
             Password = "TestPass123"
         }
     };
     var (regResponse, regResult) = await client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);
     
     // Login via Identity endpoint (raw HttpClient)
     // SRV007: Using raw HttpClient.PostAsJsonAsync is necessary to test ASP.NET Identity
     // endpoints which are not FastEndpoints. ASP.NET Identity endpoints are mapped via
     // MapIdentityApi and have different request/response structures. FastEndpoints testing
     // extensions (POSTAsync, GETAsync) only work with FastEndpoints-based endpoints.
     #pragma warning disable SRV007
     var identityLoginResponse = await client.PostAsJsonAsync("/api/identity/login", new
     {
         email = "test@example.com",
         password = "TestPass123"
     });
     #pragma warning restore SRV007
     
     // Verify login succeeded and cookie is set
     identityLoginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
     var cookies = identityLoginResponse.Headers.GetValues("Set-Cookie");
     cookies.ShouldNotBeEmpty();
     ```
   - Test: Register via `/api/users/register`, call `/api/user` with Token auth:
     ```csharp
     // Use FastEndpoints POSTAsync for legacy registration
     var (regResponse, regResult) = await client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request);
     
     // Extract token and set Authorization header
     client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", regResult.User.Token);
     
     // Call /api/user
     var (userResponse, userResult) = await client.GETAsync<GetCurrent, GetCurrentResponse>();
     
     // Verify user data is retrieved correctly
     userResult.User.Email.ShouldBe(request.User.Email);
     userResult.User.Username.ShouldBe(request.User.Username);
     ```

8. **Create Functional Tests for Cross-Authentication (Identity Endpoint -> Legacy Login)**
   - In `CrossAuthenticationTests.cs`, add tests for opposite direction
   - Test: Register via `/api/identity/register`, then login via `/api/users/login`:
     ```csharp
     // Register with Identity (raw HttpClient)
     #pragma warning disable SRV007
     var identityRegResponse = await client.PostAsJsonAsync("/api/identity/register", new
     {
         email = "test@example.com",
         password = "TestPass123"
     });
     #pragma warning restore SRV007
     identityRegResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
     
     // Login via legacy endpoint (FastEndpoints)
     var loginRequest = new LoginRequest
     {
         User = new LoginUserData
         {
             Email = "test@example.com",
             Password = "TestPass123"
         }
     };
     var (loginResponse, loginResult) = await client.POSTAsync<Login, LoginRequest, LoginResponse>(loginRequest);
     
     // Verify login succeeded and JWT token is returned
     loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
     loginResult.User.Token.ShouldNotBeNullOrEmpty();
     ```
   - Test: Register via `/api/identity/register`, call `/api/user` with cookie auth:
     ```csharp
     // Register with Identity (sets cookie automatically)
     #pragma warning disable SRV007
     var identityRegResponse = await client.PostAsJsonAsync("/api/identity/register", new
     {
         email = "test@example.com",
         password = "TestPass123"
     });
     #pragma warning restore SRV007
     
     // Cookie is automatically sent with subsequent requests
     var (userResponse, userResult) = await client.GETAsync<GetCurrent, GetCurrentResponse>();
     
     // Verify user data is retrieved correctly via cookie auth
     userResult.User.Email.ShouldBe("test@example.com");
     ```

9. **Update Test Fixtures for Cookie Support**
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

10. **Remove Legacy User Entity and Related Code**
    - After all endpoints are migrated to use ApplicationUser:
      - Remove legacy User entity from `Server.Core/UserAggregate/User.cs`
      - Remove legacy password hasher interfaces and implementations
      - Remove any User-related specifications that query the old Users table
      - Remove legacy authentication services that are no longer used
    - Update EF Core DbContext:
      - Remove `DbSet<User> Users` from AppDbContext
      - Remove User entity configuration
    - Create and apply EF Core migration to drop the Users table
    - Note: This step removes the legacy dual-table setup

11. **Run and Fix Tests**
    - Run `./build.sh TestServer` to execute all functional tests
    - Fix any issues with:
      - Endpoint response formats
      - Authentication flows
      - Cookie handling in tests
      - Database queries expecting legacy User entity
    - Ensure no regressions in existing tests

12. **Verify Dual Authentication Works**
    - Manually test the complete flows:
      - Register via /api/users -> login via /api/identity
      - Register via /api/identity -> login via /api/users
      - Access protected endpoints with both auth types
    - Verify only AspNetUsers table is used (no legacy Users table)
    - Verify no data loss or issues with user operations

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh TestServer
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass. The existing /api/users endpoints now use ASP.NET Identity internally (UserManager and SignInManager) while maintaining backward compatibility. Functional tests validate cross-authentication scenarios between /api/users and /api/identity endpoints. Postman and E2E tests continue to work with /api/users endpoints without changes.