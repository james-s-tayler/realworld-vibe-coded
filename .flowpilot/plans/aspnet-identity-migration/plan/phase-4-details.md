## phase_4

### Phase Overview

Update functional integration tests to use Identity endpoints and cookie-based authentication instead of JWT tokens. Tests will call /register and /login endpoints and use HttpClient with cookie support to maintain authentication state across requests. All functional tests should pass with the new authentication system.

### Prerequisites

- Phase 3 completed: Identity API endpoints available via MapIdentityApi
- Both authentication systems working simultaneously
- Manual testing confirms Identity endpoints work correctly

### Implementation Steps

1. **Update Test Authentication Helper Methods**
   - Open test fixtures (e.g., `Server.FunctionalTests/UsersFixture.cs`, `ArticlesFixture.cs`)
   - Update `CreateUserAndLogin` helper methods to use Identity endpoints:
     - Change registration from `POST /api/users` to `POST /register`
     - Change login from `POST /api/users/login` to `POST /login`
     - Update request/response DTOs to match Identity's structure
     - Remove JWT token extraction from response body
     - Return user information instead of token

2. **Update HttpClient Creation for Cookie Support**
   - In test fixtures, update `CreateClient` methods to support cookies:
     ```csharp
     protected HttpClient CreateAuthenticatedClient(string email, string password)
     {
         var client = CreateClient(new WebApplicationFactoryClientOptions
         {
             AllowAutoRedirect = false,
             HandleCookies = true  // Enable cookie handling
         });
         
         // Register and login to get authentication cookie
         var registerResponse = client.PostAsJsonAsync("/register", new 
         {
             email = email,
             password = password
         }).GetAwaiter().GetResult();
         
         // Cookie is automatically stored in client
         return client;
     }
     ```

3. **Remove JWT Token Authorization Headers**
   - Remove code that adds `Authorization: Token <jwt>` headers
   - Remove JWT token storage in test fixtures
   - Remove GetCurrentToken() calls from tests
   - Cookies will be automatically sent with requests

4. **Update Users Tests**
   - Open `Server.FunctionalTests/Users/UsersTests.cs`
   - Update test methods to use new endpoints:
     - Registration tests use `/register`
     - Login tests use `/login`
     - Get current user tests may need adjustment (Identity's `/manage/info` endpoint)
     - Update user tests may need adjustment
   - Update expected request/response formats to match Identity
   - Verify all user tests pass

5. **Update Articles Tests Authentication**
   - Open article test files (e.g., `ArticlesTests.cs`)
   - Update `ArticlesFixture` to use cookie-based authentication
   - Update test setup methods to create authenticated clients with cookies
   - Verify article tests that require authentication still pass
   - No changes needed to article endpoint tests themselves (only auth setup)

6. **Update Other Test Fixtures**
   - Update any other test fixtures that create authenticated clients:
     - Comments tests
     - Profiles tests (follow/unfollow)
     - Tags tests (if any require auth)
   - Follow same pattern: use cookie-based clients instead of JWT tokens

7. **Handle Bio and Image in Registration**
   - Identity's /register endpoint doesn't support custom fields like Bio and Image
   - For tests that need these, make a second request after registration to update profile
   - Or create a custom registration helper that calls /register then updates profile

8. **Run and Fix Failing Tests**
   - Run `./build.sh TestServer` to execute all functional tests
   - Fix any failing tests by adjusting request/response formats
   - Common issues to watch for:
     - Different error response formats
     - Different validation messages
     - Different HTTP status codes
     - Cookie handling issues

9. **Verify Test Coverage**
   - Ensure all test scenarios from old system are covered:
     - Registration with valid/invalid data
     - Login with correct/incorrect credentials
     - Authenticated operations work with cookies
     - Unauthenticated operations return 401
     - Invalid tokens/cookies return appropriate errors

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
```

All targets must pass. Functional tests should now use Identity endpoints and cookie authentication. Postman and E2E tests still use old JWT authentication and continue to pass.