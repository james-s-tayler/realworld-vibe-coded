## phase_5

### Phase Overview

Update the Postman collection to use Identity endpoints (/api/identity/register, /api/identity/login) and cookie-based authentication instead of JWT tokens. Configure Postman to automatically send and receive cookies, and update all authentication-related requests to match Identity's API structure.

### Prerequisites

- Phase 4 completed: Functional tests updated and passing with Identity
- Identity endpoints fully functional and tested
- Dual authentication still active

### Implementation Steps

1. **Locate Postman Collection**
   - Find the Postman collection file (typically in `Test/postman/` directory)
   - Open in Postman or a text editor
   - Review current authentication flow (JWT-based)

2. **Update Registration Requests**
   - Find registration test requests (located in the Auth folder in the postman collection)
   - Update to use Identity endpoint:
     - Change URL from `/api/users/register` to `/api/identity/register`
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
   - Find login test requests (POST /api/users/login)
   - Update to use Identity endpoint:
     - Change URL from `/api/users/login` to `/api/identity/login`
     - Update request body:
       ```json
       {
         "email": "user@example.com",
         "password": "StrongPass123"
       }
       ```
     - Remove token extraction from response body

4. **Remove JWT Token Variables and Headers**
   - Remove environment variables for JWT tokens
   - Remove Authorization header with Token prefix from requests
   - Remove any pre-request scripts that set JWT tokens
   - Postman will automatically send cookies with subsequent requests

5. **Configure Postman Cookie Settings**
   - See documentation on using cookies in postman scripts https://learning.postman.com/docs/sending-requests/response-data/cookies/#script-with-cookies
   - Ensure Postman collection is configured to handle cookies:
     - In collection settings, verify "Automatically follow redirects" is off (we want 401/403, not redirects)
     - Verify "Enable cookie jar" is on
   - Test that cookies persist across requests in a collection run
   - Ensure postman collection is configured to handle cookies
   - Run `FOLDER=Auth ./build.sh TestServerPostman` to ensure the Auth related Postman tests are passing first before proceeding.

6. **Update All Other Tests in Postman collection to use cookies**
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
./build.sh TestE2e
```

All targets must pass. Postman collection should now use Identity endpoints with cookie authentication. E2E tests still use old /api/users/register and /api/users/login with JWT authentication and continue to pass.