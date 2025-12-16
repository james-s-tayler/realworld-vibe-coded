## phase_10

### Phase Overview

Update the frontend and E2E tests to remove username from calls to /api/users/register. Remove the username field from the registration form. After this phase, no code in the system explicitly provides username - it always defaults to email.

### Prerequisites

- Phase 9 completed: All postman collections updated and passing without username
- Backend supports optional username (defaults to email)
- Frontend and E2E tests still providing username explicitly

### Implementation Steps

1. **Update Frontend Registration Form**
   - Open the registration page/component (likely in `App/Client/src/pages/` or `App/Client/src/components/`)
   - Remove username input field from the form
   - Update form state to only include email and password
   - Remove username validation from the form

2. **Update Frontend API Client - Register Function**
   - Open `App/Client/src/api/auth.ts` (or similar authentication API file)
   - Update the register function to only send email and password:
     ```typescript
     register: async (email: string, password: string) => {
       const response = await apiRequest('/api/users/register', {
         method: 'POST',
         body: JSON.stringify({
           user: { email, password }
         }),
       });
       return response;
     }
     ```
   - Remove username parameter from function signature

3. **Update AuthContext Registration**
   - Open `App/Client/src/context/AuthContext.tsx` (or similar)
   - Update register method to only accept email and password
   - Remove username from user data model if it's separate from email
   - Update any references to username in the auth context

4. **Update Frontend User Model (if needed)**
   - Review user type definitions (likely in `App/Client/src/types/`)
   - Ensure username field is still present (it will be populated by backend with email value)
   - No changes should be needed if username comes from API responses

5. **Test Frontend Registration**
   - Run `./build.sh BuildClient` to build frontend
   - Manually test registration in the browser
   - Verify registration works without username field
   - Verify registered user has username equal to email

6. **Update E2E Test Helpers - Registration Methods**
   - Open E2E test base classes or helper files (e.g., `Test/e2e/E2eTests/Common/AppFixture.cs`)
   - Find methods that create users via API or UI
   - Update to not provide username parameter
   - Example API call:
     ```csharp
     var registerRequest = new { user = new { email, password } };
     await client.PostAsJsonAsync("/api/users/register", registerRequest);
     ```

7. **Update E2E Page Objects**
   - Open page object files (e.g., `Test/e2e/E2eTests/Pages/RegisterPage.cs`)
   - Remove username parameter from registration methods
   - Update form filling to only fill email and password
   - Remove username field locators

8. **Update E2E Test Cases**
   - Open test files in `Test/e2e/E2eTests/`
   - Update any test-specific user creation code
   - Remove username from test data
   - Update assertions to expect username equal to email

9. **Run E2E Tests**
   - Run `./build.sh TestE2e`
   - Verify all E2E tests pass
   - Fix any failing tests related to username removal

10. **Verify All Tests Pass**
    - Run `./build.sh TestServer` - functional tests
    - Run `./build.sh TestServerPostman` - postman tests
    - Run `./build.sh TestE2e` - E2E tests
    - All tests should pass

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

All targets must pass. No code in the system (frontend, E2E tests, postman collections) should provide username explicitly. All users are registered with email as username.
