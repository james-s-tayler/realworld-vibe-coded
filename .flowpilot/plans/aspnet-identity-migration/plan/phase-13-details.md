## phase_13

### Phase Overview

Configure the backend to support Identity's bearer token authentication scheme in addition to the existing "Token" scheme and Identity cookie scheme. Update the Auth postman collection to switch to calling /api/identity/register and /api/identity/login endpoints using the standard Bearer token scheme.

### Prerequisites

- Phase 12 completed: All tests using two-step authentication flow
- Backend has dual authentication (JWT "Token" scheme and Identity endpoints)
- All tests currently using /api/users endpoints

### Implementation Steps

1. **Review Current Authentication Configuration**
   - Open `App/Server/src/Server.Web/Configurations/ServiceConfigs.cs`
   - Review current authentication schemes:
     - "Token" scheme for JWT (Authorization: Token <jwt>)
     - Identity cookie scheme
   - Identity bearer token scheme may already be configured from phase 3/4

2. **Ensure Identity Bearer Token Scheme is Configured**
   - Verify `MapIdentityApi<ApplicationUser>()` is called (should be from phase 3)
   - This automatically configures bearer token authentication
   - Verify multiple authentication schemes can be used simultaneously:
     ```csharp
     builder.Services.AddAuthentication(options =>
     {
       options.DefaultScheme = "Token"; // Keep existing as default
       options.DefaultChallengeScheme = "Token";
     })
     .AddJwtBearer("Token", options => { /* existing config */ })
     .AddCookie(IdentityConstants.ApplicationScheme); // Identity cookies
     
     // Identity bearer token configured via MapIdentityApi
     ```

3. **Update FastEndpoints to Support Multiple Schemes**
   - Open endpoint files (if needed)
   - Verify endpoints support all three schemes:
     - "Token" (JWT) for backward compatibility
     - IdentityConstants.BearerScheme for Identity bearer tokens
     - IdentityConstants.ApplicationScheme for Identity cookies
   - Most endpoints should accept any valid authentication
   - No explicit changes may be needed if [Authorize] is used generically

4. **Test Identity Endpoints with Postman**
   - Manually test in Postman:
     - POST /api/identity/register with `{ "email": "user@example.com", "password": "password123" }`
     - POST /api/identity/login with `{ "email": "user@example.com", "password": "password123" }`
     - Verify login returns accessToken in response
     - Use accessToken with "Authorization: Bearer <token>" header
     - Make authenticated request to verify bearer token works

5. **Update Auth Postman Collection - Register Endpoint**
   - Open `Test/Postman/Conduit.Auth.postman_collection.json`
   - Find register requests using /api/users/register
   - Update to use /api/identity/register:
     ```json
     {
       "email": "{{email}}",
       "password": "{{password}}"
     }
     ```
   - Note: Identity register doesn't return a token (we already handle this in phase 12)

6. **Update Auth Postman Collection - Login Endpoint**
   - Find login requests using /api/users/login
   - Update to use /api/identity/login:
     ```json
     {
       "email": "{{email}}",
       "password": "{{password}}"
     }
     ```
   - Update test scripts to extract accessToken from response:
     ```javascript
     var jsonData = pm.response.json();
     pm.environment.set("token", jsonData.accessToken);
     ```

7. **Update Auth Collection - Authorization Headers**
   - Find requests that use "Authorization: Token {{token}}" header
   - Update to use standard bearer scheme: "Authorization: Bearer {{token}}"
   - Or set at collection level if all requests need it

8. **Update Auth Collection - Test Assertions**
   - Review test scripts in Auth collection
   - Update any assertions that check token format or presence
   - Ensure tests validate Identity endpoint responses correctly

9. **Test Auth Collection with Identity Endpoints**
   - Run `FOLDER=Auth ./build.sh TestServerPostman`
   - Verify all Auth tests pass with Identity endpoints
   - Debug any failures related to endpoint differences

10. **Verify Other Collections Still Pass**
    - Run `FOLDER=Profiles ./build.sh TestServerPostman`
    - Run `FOLDER=FeedAndArticles ./build.sh TestServerPostman`
    - Run `FOLDER=Article ./build.sh TestServerPostman`
    - Other collections still use /api/users endpoints (not yet migrated)
    - They should still pass

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
FOLDER=Auth ./build.sh TestServerPostman
FOLDER=Profiles ./build.sh TestServerPostman
FOLDER=FeedAndArticles ./build.sh TestServerPostman
FOLDER=Article ./build.sh TestServerPostman
FOLDER=ArticlesEmpty ./build.sh TestServerPostman
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass. Auth collection should use Identity endpoints with Bearer token authentication. Other collections still use /api/users endpoints. Backend supports all three authentication schemes.
