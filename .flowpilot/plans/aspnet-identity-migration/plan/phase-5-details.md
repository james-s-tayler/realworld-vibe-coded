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

6. **Update User Profile Tests**
   - Update "Get Current User" tests:
     - Identity's equivalent is GET `/manage/info`
     - Or use the existing /api/user endpoint if it works with cookie auth
   - Update "Update User" tests to work with cookies instead of JWT
   - Verify authenticated requests work with cookies

7. **Update Tests for Other Entities**
   - Review and update tests for articles, comments, profiles that require authentication
   - Ensure they rely on cookies instead of JWT tokens
   - No URL changes needed for non-auth endpoints
   - Just ensure cookie authentication works

8. **Add Cookie-Specific Tests**
   - Add test cases for cookie scenarios:
     - Test that unauthenticated requests return 401
     - Test that expired cookies return 401
     - Test that logout clears cookies
   - Consider adding a logout test using POST `/logout` if Identity provides it

9. **Test Postman Collection**
   - Run the full Postman collection in Postman UI:
     - Verify registration works
     - Verify login works and sets cookies
     - Verify authenticated requests succeed
     - Verify unauthenticated requests fail with 401
   - Fix any failing tests

10. **Run Automated Postman Tests**
    - Run `./build.sh TestServerPostman`
    - Investigate and fix any failures
    - Ensure all Postman tests pass

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostman
```

All targets must pass. Postman collection should now use Identity endpoints with cookie authentication. E2E tests still use old JWT authentication and continue to pass.