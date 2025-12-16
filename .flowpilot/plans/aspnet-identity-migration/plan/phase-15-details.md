## phase_15

### Phase Overview

Update the frontend and E2E tests to switch to calling /api/identity/register and /api/identity/login endpoints using the Identity bearer token scheme. After this phase, the entire system (frontend, backend, all tests) uses Identity endpoints with bearer tokens, preparing for the final switch to cookie-based authentication.

### Prerequisites

- Phase 14 completed: All postman collections using Identity endpoints
- Backend supports three authentication schemes
- Frontend and E2E tests still using /api/users endpoints

### Implementation Steps

1. **Update Frontend API Client - Register Endpoint**
   - Open `App/Client/src/api/auth.ts` (or similar authentication API file)
   - Update register function to use /api/identity/register:
     ```typescript
     register: async (email: string, password: string) => {
       await apiRequest('/identity/register', {
         method: 'POST',
         body: JSON.stringify({ email, password }),
       });
       // No token returned from Identity register
     }
     ```

2. **Update Frontend API Client - Login Endpoint**
   - Update login function to use /api/identity/login:
     ```typescript
     login: async (email: string, password: string) => {
       const response = await apiRequest('/identity/login', {
         method: 'POST',
         body: JSON.stringify({ email, password }),
       });
       return response.accessToken; // Identity returns accessToken, not token
     }
     ```

3. **Update Frontend Authorization Headers**
   - Open `App/Client/src/api/client.ts` (or base API client)
   - Update to use standard Bearer scheme instead of "Token" scheme:
     ```typescript
     headers: {
       'Authorization': `Bearer ${token}`, // Changed from `Token ${token}`
       'Content-Type': 'application/json',
     }
     ```

4. **Update AuthContext/AuthProvider**
   - Open `App/Client/src/context/AuthContext.tsx` (or similar)
   - Update login/register methods to use new API endpoints
   - Update token handling to use accessToken field from response
   - Ensure two-step flow still works (register then login)

5. **Test Frontend Locally**
   - Run `./build.sh BuildClient` to build frontend
   - Start the application and test in browser:
     - Register a new user - verify it works
     - Login - verify it works
     - Make authenticated requests - verify they work
     - Check browser DevTools network tab to verify Bearer tokens used
     - Verify no console errors

6. **Update E2E Test Helpers - Authentication Methods**
   - Open E2E test fixture files (e.g., `Test/e2e/E2eTests/ApiFixture.cs`)
   - Update CreateUserAndLogin to use Identity endpoints:
     ```csharp
     public async Task<string> CreateUserAndLogin(string username, string email, string password)
     {
       // Register via Identity
       var registerRequest = new { email, password };
       await client.PostAsJsonAsync("/api/identity/register", registerRequest);
       
       // Login via Identity to get token
       var loginRequest = new { email, password };
       var loginResponse = await client.PostAsJsonAsync("/api/identity/login", loginRequest);
       var result = await loginResponse.Content.ReadFromJsonAsync<IdentityLoginResponse>();
       return result.AccessToken; // Identity uses accessToken field
     }
     ```

7. **Update E2E Test Authorization Headers**
   - Update HttpClient configuration to use Bearer scheme:
     ```csharp
     client.DefaultRequestHeaders.Authorization = 
       new AuthenticationHeaderValue("Bearer", token); // Changed from "Token"
     ```

8. **Update E2E Page Objects**
   - Open registration/login page objects
   - Update to work with Identity endpoints if needed
   - Verify form interactions still work
   - Update any endpoint-specific logic

9. **Update E2E Test Cases**
   - Open test files in `Test/e2e/E2eTests/`
   - Review any hardcoded endpoint references
   - Update test assertions if needed
   - Verify test data works with Identity endpoints

10. **Run E2E Tests**
    - Run `./build.sh TestE2e`
    - Verify all E2E tests pass with Identity endpoints
    - Fix any failing tests

11. **Run Frontend Unit Tests**
    - Run `./build.sh TestClient`
    - Verify frontend unit tests pass
    - Update any mocked API responses if needed

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintClientVerify
./build.sh BuildClient
./build.sh TestClient
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass. Entire system (frontend, backend, all tests) now uses Identity endpoints with bearer token authentication. Old /api/users endpoints remain but are unused. Ready to switch to cookie authentication.
