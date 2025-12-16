## phase_11

### Phase Overview

Update the frontend and E2E tests to ignore the token in the /api/users/register response and instead make an explicit call to /api/users/login to get the token after registering. This prepares the code for switching to Identity endpoints, which don't return a token on registration.

### Prerequisites

- Phase 10 completed: Frontend and E2E tests not providing username
- All tests passing with email as username
- Registration still returns token but it will be ignored

### Implementation Steps

1. **Update Frontend API Client - Register Function**
   - Open `App/Client/src/api/auth.ts` (or similar authentication API file)
   - Update the register function to NOT extract/return token from response:
     ```typescript
     register: async (email: string, password: string) => {
       await apiRequest('/api/users/register', {
         method: 'POST',
         body: JSON.stringify({
           user: { email, password }
         }),
       });
       // No token extraction - will login separately
     }
     ```
   - Function should return void or just success indicator

2. **Update AuthContext Register Method**
   - Open `App/Client/src/context/AuthContext.tsx` (or similar)
   - Update register method to call login after successful registration:
     ```typescript
     const register = async (email: string, password: string) => {
       await authApi.register(email, password);
       // Now login to get the token
       await login(email, password);
     };
     ```
   - This establishes the two-step pattern: register then login

3. **Update Frontend Registration Components**
   - Open registration page/component
   - Verify it calls the updated register method from AuthContext
   - Ensure success handling works (user should be logged in after register)
   - Verify error handling still works

4. **Test Frontend Registration Flow**
   - Run `./build.sh BuildClient` to build frontend
   - Manually test registration in browser:
     - Register a new user
     - Verify user is registered AND logged in after registration
     - Verify no errors in console
     - Verify authenticated requests work after registration

5. **Update E2E Test Helpers - Registration Methods**
   - Open E2E test fixture files (e.g., `Test/e2e/E2eTests/ApiFixture.cs`)
   - Update helper methods that register users:
     ```csharp
     public async Task<string> CreateUserAndLogin(string username, string email, string password)
     {
       // Register
       var registerRequest = new { user = new { email, password } };
       await client.PostAsJsonAsync("/api/users/register", registerRequest);
       
       // Login to get token
       var loginRequest = new { user = new { email, password } };
       var loginResponse = await client.PostAsJsonAsync("/api/users/login", loginRequest);
       var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
       return result.User.Token;
     }
     ```

6. **Update E2E Page Objects**
   - Open registration page object (e.g., `Test/e2e/E2eTests/PageModels/RegisterPage.cs`)
   - If registration automatically logs in user, ensure page object reflects this
   - Update any methods that expect token from registration

7. **Update E2E Test Cases**
   - Open test files in `Test/e2e/E2eTests/`
   - Review tests that register users
   - Ensure they work with two-step flow (register then login)
   - Update test assertions if needed

8. **Run E2E Tests**
   - Run `./build.sh TestE2e`
   - Verify all tests pass with two-step authentication flow
   - Fix any failing tests

9. **Verify Frontend Tests Pass**
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

All targets must pass. Frontend and E2E tests should use two-step authentication: register then login. Postman collections still use single-step (token from register response). This is acceptable at this stage.
