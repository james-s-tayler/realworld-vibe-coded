## phase_3

### Phase Overview

Add Identity API endpoints using MapIdentityApi<ApplicationUser>() to expose registration, login, and other authentication endpoints. Both the new Identity endpoints and the existing JWT-based endpoints operate simultaneously, allowing for gradual migration of tests and frontend without breaking changes.

### Prerequisites

- Phase 2 completed: Identity services configured, cookie authentication set up
- All tests passing with old JWT authentication
- Application builds and runs successfully

### Implementation Steps

1. **Add MapIdentityApi Endpoint Mapping**
   - Open `Server.Web/Program.cs` or the location where endpoints are configured
   - After app.UseAuthorization() but before app.UseFastEndpoints(), add:
     ```csharp
     // Map Identity API endpoints (Decision 3: Use MapIdentityApi Directly)
     app.MapIdentityApi<ApplicationUser>();
     ```
   - This exposes Identity endpoints at standard paths:
     - POST /register
     - POST /login
     - POST /refresh
     - GET /confirmEmail
     - POST /resendConfirmationEmail
     - POST /forgotPassword
     - POST /resetPassword
     - POST /manage/2fa
     - GET /manage/info

2. **Verify Dual Authentication Support**
   - Confirm that middleware pipeline supports both authentication schemes:
     - JWT Bearer authentication (existing, "Token" scheme)
     - Cookie authentication (Identity, default scheme)
   - Both schemes should be evaluated by the authentication middleware
   - Endpoints can specify which scheme they require or accept both

3. **Test Identity Endpoints Manually**
   - Start the application
   - Use curl or Postman to test new Identity endpoints:
     ```bash
     # Register a new user
     curl -X POST http://localhost:5000/register \
       -H "Content-Type: application/json" \
       -d '{"email":"test@test.com","password":"TestPass123"}'
     
     # Login
     curl -X POST http://localhost:5000/login \
       -H "Content-Type: application/json" \
       -d '{"email":"test@test.com","password":"TestPass123"}'
     ```
   - Verify responses return 200 OK and set cookies
   - Note: Custom properties (Bio, Image) won't be in responses yet - that's expected

4. **Verify Old Endpoints Still Work**
   - Test existing JWT authentication endpoints:
     ```bash
     # Register with old endpoint
     POST /api/users
     
     # Login with old endpoint
     POST /api/users/login
     ```
   - Verify they still return JWT tokens in response body
   - Confirm old tests continue to pass

5. **Document Endpoint Coexistence**
   - Add a comment in code documenting that both systems are temporarily active:
     ```csharp
     // TEMPORARY: Both Identity (cookie-based) and legacy JWT authentication are active
     // during migration. Old endpoints: /api/users, /api/users/login
     // New endpoints: /register, /login (via MapIdentityApi)
     // This will be cleaned up in Phase 7 after all tests are migrated.
     ```

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass. The application should support both authentication systems simultaneously. Old tests continue passing with JWT authentication. New Identity endpoints are functional and can be tested manually.