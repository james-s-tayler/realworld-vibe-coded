## phase_16

### Phase Overview

Add CSRF protection to the backend and switch the frontend to use cookie-based authentication. Add a /logout endpoint using SignInManager. This establishes production-ready authentication with secure cookies and CSRF protection, while maintaining bearer token support for backward compatibility.

### Prerequisites

- Phase 15 completed: All tests using Identity endpoints with bearer tokens
- Backend supports Identity bearer tokens and cookies
- Frontend using bearer tokens (not cookies yet)

### Implementation Steps

1. **Configure CSRF Protection on Backend**
   - Open `App/Server/src/Server.Web/Configurations/ServiceConfigs.cs`
   - Add CSRF protection configuration:
     ```csharp
     builder.Services.AddAntiforgery(options =>
     {
       options.HeaderName = "X-XSRF-TOKEN";
       options.Cookie.Name = "XSRF-TOKEN";
       options.Cookie.HttpOnly = false; // Client needs to read it
       options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
       options.Cookie.SameSite = SameSiteMode.Strict;
     });
     ```
   - Configure middleware to validate antiforgery tokens

2. **Create Logout Endpoint**
   - Create new endpoint file: `App/Server/src/Server.Web/Identity/Logout/Logout.cs`
   - Implement logout using SignInManager:
     ```csharp
     public class Logout : EndpointWithoutRequest
     {
       private readonly SignInManager<ApplicationUser> _signInManager;
       
       public override void Configure()
       {
         Post("/api/identity/logout");
         AllowAnonymous(); // Or require auth
       }
       
       public override async Task HandleAsync(CancellationToken ct)
       {
         await _signInManager.SignOutAsync();
         await SendOkAsync(ct);
       }
     }
     ```

3. **Test Logout Endpoint**
   - Manually test logout with Postman or curl
   - Verify cookies are cleared after logout
   - Verify authenticated requests fail after logout

4. **Update Frontend to Use Cookie Authentication**
   - Open `App/Client/src/api/auth.ts`
   - Update login to use cookie authentication:
     ```typescript
     login: async (email: string, password: string) => {
       const response = await apiRequest('/identity/login?useCookies=true', {
         method: 'POST',
         body: JSON.stringify({ email, password }),
       });
       // No token to extract - cookie automatically set by browser
       return response;
     }
     ```

5. **Update Frontend to Remove Token Storage**
   - Open `App/Client/src/api/client.ts`
   - Remove Authorization header code (cookies sent automatically)
   - Ensure `credentials: 'include'` is set on all requests:
     ```typescript
     const response = await fetch(url, {
       ...options,
       credentials: 'include', // Send cookies
     });
     ```

6. **Update AuthContext for Cookie Auth**
   - Open `App/Client/src/context/AuthContext.tsx`
   - Remove token state management
   - Update login to not return token
   - Add logout method that calls /api/identity/logout
   - After login, fetch user info from /api/user endpoint

7. **Update Frontend to Handle CSRF Tokens**
   - Implement CSRF token extraction from cookie
   - Add X-XSRF-TOKEN header to mutating requests (POST, PUT, DELETE)
   - Read XSRF-TOKEN cookie and include in header:
     ```typescript
     const csrfToken = getCookie('XSRF-TOKEN');
     headers['X-XSRF-TOKEN'] = csrfToken;
     ```

8. **Test Frontend with Cookie Auth**
   - Run `./build.sh BuildClient`
   - Manually test in browser:
     - Register and login - verify cookies set
     - Make authenticated requests - verify they work
     - Logout - verify cookies cleared
     - Check browser DevTools to see auth cookies and CSRF cookie
     - Verify no bearer tokens in Authorization headers

9. **Update E2E Tests for Cookie Auth**
   - Open E2E test fixture files
   - Update to use cookie authentication:
     - Login with ?useCookies=true parameter
     - Remove bearer token handling
     - Playwright handles cookies automatically
   - May need to handle CSRF tokens in API calls

10. **Run E2E Tests**
    - Run `./build.sh TestE2e`
    - Verify all E2E tests pass with cookie authentication
    - Fix any failures related to cookie/CSRF handling

11. **Update Frontend Tests**
    - Run `./build.sh TestClient`
    - Update any mocked API calls if needed
    - Verify all frontend tests pass

12. **Verify All Tests Pass**
    - Run `./build.sh TestServer` - functional tests (still use bearer tokens)
    - Run `./build.sh TestServerPostman` - postman tests (still use bearer tokens)
    - Run `./build.sh TestE2e` - E2E tests (now use cookies)
    - Backend supports both bearer tokens and cookies simultaneously

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

All targets must pass. Frontend and E2E tests use cookie authentication with CSRF protection. Postman tests still use bearer tokens (acceptable). Backend supports both authentication methods. System is production-ready.
