## phase_6

### Phase Overview

Update the frontend React application and Playwright E2E tests to use Identity endpoints with cookie-based authentication. Remove JWT token handling from the frontend, update API client to call /register and /login endpoints, and rely on automatic browser cookie management. Update E2E tests to work with the new authentication flows.

### Prerequisites

- Phase 5 completed: Postman tests updated and passing with Identity
- Backend fully supports cookie authentication
- All backend tests passing with Identity

### Implementation Steps

1. **Update Frontend API Client - Authentication Module**
   - Open `App/Client/src/api/auth.ts` (or similar authentication API file)
   - Update registration function:
     ```typescript
     export const authApi = {
       register: async (email: string, password: string) => {
         const response = await apiRequest('/register', {
           method: 'POST',
           body: JSON.stringify({ email, password }),
         });
         return response; // Cookie automatically set by browser
       },
       // ... other methods
     }
     ```
   - Update login function similarly to use `/login` endpoint
   - Remove JWT token extraction from responses
   - Remove token storage in localStorage/sessionStorage

2. **Remove JWT Token Authorization Headers**
   - Open `App/Client/src/api/client.ts` (or base API client)
   - Remove code that adds `Authorization: Token <jwt>` header
   - Remove token retrieval from storage
   - Browser will automatically send cookies with requests
   - Ensure `credentials: 'include'` is set on fetch requests:
     ```typescript
     fetch(url, {
       ...options,
       credentials: 'include', // Send cookies with requests
     })
     ```

3. **Update AuthContext/AuthProvider**
   - Open `App/Client/src/context/AuthContext.tsx` (or similar)
   - Remove JWT token state management
   - Update login/register methods to use new API endpoints
   - Update logout method to call Identity's logout endpoint (if available) or just clear client state
   - Remove token refresh logic (cookies handle this automatically)

4. **Update User State Management**
   - After login/register, fetch user info from Identity's `/manage/info` endpoint
   - Or use existing `/api/user` endpoint if it works with cookie auth
   - Store user data in context/state as before, just without token
   - Update getCurrentUser to rely on cookies for authentication

5. **Remove Token from API Calls**
   - Search frontend codebase for references to "Authorization" header or "Token"
   - Remove all manual token header setting
   - Verify all API calls use the base client that includes credentials
   - Test that authenticated requests work with cookies only

6. **Handle 401 Responses**
   - Ensure frontend properly handles 401 Unauthorized responses
   - Redirect to login page on 401 (user not authenticated)
   - Don't try to refresh tokens - cookies are managed by server
   - Clear user state on 401 and redirect to login

7. **Update Playwright E2E Tests - Authentication Helpers**
   - Open E2E test base classes or helper files (e.g., `Test/e2e/E2eTests/Common/BasePage.cs` or similar)
   - **Note**: `ApiFixture` will need to be updated to support cookie-based authentication
   - Update authentication methods:
     - Change registration to use /register endpoint
     - Change login to use /login endpoint
     - Remove JWT token handling
     - Playwright automatically handles cookies
   - Update page object methods that were passing tokens

8. **Update Playwright Test Cases**
   - Open test files in `Test/e2e/E2eTests/`
   - Update any test-specific authentication logic
   - Remove token assertions from tests
   - Add cookie assertions if needed (verify auth cookie is set)
   - Verify authenticated tests still work with cookies

9. **Test Frontend Locally**
   - Run `./build.sh BuildClient` to build frontend
   - Start the application and test in browser:
     - Register a new user - verify it works
     - Login - verify it works and no errors in console
     - Navigate to protected pages - verify they work
     - Logout - verify it clears auth state
     - Check browser DevTools to see auth cookies

10. **Run E2E Tests**
    - Run `./build.sh TestE2e`
    - Fix any failing tests
    - Common issues:
      - Timing issues with cookie setting
      - Page navigation before auth completes
      - Incorrect endpoint URLs
      - Missing credentials: 'include' in requests

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

All targets must pass. Frontend and E2E tests should now use cookie-based authentication. The entire system (backend, frontend, all tests) now uses Identity exclusively. Legacy JWT endpoints are still present but unused.