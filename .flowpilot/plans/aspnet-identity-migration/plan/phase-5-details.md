## phase_5

### Phase Overview

Update the Postman collection to use Identity endpoints (/register, /login) with cookie-based authentication instead of JWT tokens. Configure Postman to automatically send and receive cookies, and update all authentication-related requests to match Identity's API structure.

### Prerequisites

- Phase 4 completed: Functional tests updated and passing with Identity
- Identity endpoints fully functional and tested
- Dual authentication still active

### Implementation Steps

1. **Locate Postman Collection**
   - Find the Postman collection file (typically in `Test/postman/` directory)
   - Open in Postman or a text editor
   - Review current authentication flow (likely JWT-based)

2. **Update Registration Requests**
   - Find registration test requests (likely POST /api/users)
   - Update to use Identity endpoint:
     - Change URL from `/api/users` to `/register`
     - Update request body to match Identity's format:
       ```json
       {
         "email": "user@example.com",
         "password": "StrongPass123"
       }
       ```
     - Remove any token extraction from response
     - Verify Postman is configured to save cookies

3. **Update Login Requests**
   - Find login test requests (likely POST /api/users/login)
   - Update to use Identity endpoint:
     - Change URL from `/api/users/login` to `/login`
     - Update request body:
       ```json
       {
         "email": "user@example.com",
         "password": "StrongPass123"
       }
       ```
     - Remove token extraction from response body
     - Cookies will be automatically stored by Postman

4. **Remove JWT Token Variables and Headers**
   - Remove environment variables for JWT tokens
   - Remove Authorization header with Token prefix from requests
   - Remove any pre-request scripts that set JWT tokens
   - Postman will automatically send cookies with subsequent requests

5. **Configure Postman Cookie Settings**
   - Ensure Postman collection is configured to handle cookies:
     - In collection settings, verify "Automatically follow redirects" is off (we want 401/403, not redirects)
     - Verify "Enable cookie jar" is on
   - Test that cookies persist across requests in a collection run

6. **Update User Profile Tests - Critical Decision Point**
   - The existing `/api/user` endpoint (GetCurrent) returns custom fields (Bio, Image) that Identity's `/manage/info` doesn't provide
   - **Strategy**: Keep the existing `/api/user` endpoint but update its authentication to work with cookies
     - Update the endpoint's `AuthSchemes` configuration to accept both "Token" (JWT) and cookie authentication
     - This allows the endpoint to work with both auth methods during the migration
     - The endpoint will continue to return Bio, Image, and other custom fields
   - Update "Get Current User" Postman tests to continue using `/api/user` endpoint
   - Update "Update User" tests (`PUT /api/user`) to work with cookies instead of JWT
   - Verify authenticated requests work with cookies
   - Note: In later phases, we may decide to fully replace with Identity patterns, but for now this maintains functionality

7. **Update Tests for Other Entities**
   - Review and update tests for articles, comments, profiles that require authentication
   - Ensure they rely on cookies instead of JWT tokens
   - No URL changes needed for non-auth endpoints
   - Just ensure cookie authentication works

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostman
```

All targets must pass. Postman collection should now use Identity endpoints with cookie authentication. E2E tests still use old JWT authentication and continue to pass.