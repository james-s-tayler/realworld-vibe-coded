# References for ASP.NET Identity Migration

This document contains all research sources and references that informed the migration plan from custom JWT authentication to ASP.NET Core Identity with cookie-based authentication.

## Microsoft Learn Documentation

### [Introduction to Identity on ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-10.0)
**Accessed:** 2025-12-10

**Key Takeaways:**
- ASP.NET Core Identity is an API that supports user interface (UI) login functionality
- Manages users, passwords, profile data, roles, claims, tokens, email confirmation, and more
- Typically configured using a SQL Server database to store user names, passwords, and profile data
- Identity is not the same as Microsoft identity platform (Azure AD)
- Can be used without UI components - the UI is just Razor Pages scaffolding on top of the API

**Relevance:** Confirms that ASP.NET Identity can be used API-only without UI components, which aligns with our requirement to not use Identity UI.

### [How to use Identity to secure a Web API backend for SPAs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0)
**Accessed:** 2025-12-10

**Key Takeaways:**
- ASP.NET Core Identity provides APIs for authentication, authorization, and identity management
- Can secure endpoints with cookie-based authentication
- Use `MapIdentityApi<TUser>()` to add JSON API endpoints (`/register`, `/login`, etc.)
- Token-based option exists but cookies are recommended for browser-based apps
- Use `AddIdentityApiEndpoints<TUser>()` to configure services
- Use `AddIdentityCookies()` to establish cookie authentication

**Relevance:** This is the primary pattern we'll follow - using MapIdentityApi for endpoints and cookie authentication instead of JWT tokens.

### [Secure ASP.NET Core Blazor WebAssembly with ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-identity/?view=aspnetcore-10.0)
**Accessed:** 2025-12-10

**Key Takeaways:**
- `MapIdentityApi<TUser>()` adds JSON API endpoints for registering and logging in users
- Identity API endpoints support advanced features like two-factor authentication and email verification
- Backend server API establishes cookie authentication with `AddIdentityCookies()`
- Cookie credentials are sent with each request to the backend web API
- Example: `POST /login?useCookies=true` with email and password

**Relevance:** Demonstrates the exact pattern for cookie-based authentication with Identity API endpoints.

### [Identity model customization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-10.0)
**Accessed:** 2025-12-10

**Key Takeaways:**
- Identity model consists of entity types: User, Role, UserClaim, UserToken, UserLogin, RoleClaim, UserRole
- Can customize by deriving from IdentityDbContext and overriding OnModelCreating
- Can use custom user classes that inherit from IdentityUser
- Primary key data type can be changed (default is string, can use Guid, int, etc.)
- Use IdentityDbContext<TUser, TRole, TKey> for custom implementations
- EF Core migrations must be configured properly with Identity options

**Relevance:** We'll need to customize the Identity model to integrate with our existing User entity and support our custom fields (Bio, Image, Following relationships).

### [What's new in ASP.NET Core in .NET 8 - Identity API endpoints](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-8.0?view=aspnetcore-10.0#authentication-and-authorization)
**Accessed:** 2025-12-10

**Key Takeaways:**
- MapIdentityApi<TUser> adds `/register` and `/login` endpoints
- Main goal is to make it easy for SPA apps or Blazor apps to use ASP.NET Core Identity
- JSON API endpoints are more suitable for SPA apps than Razor Pages UI
- This is the modern approach for API-only Identity

**Relevance:** Confirms that MapIdentityApi is the recommended modern approach for API-only applications.

### [Make secure .NET Microservices and Web Applications](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/secure-net-microservices-web-applications/#implement-authentication-in-net-microservices-and-web-applications)
**Accessed:** 2025-12-10

**Key Takeaways:**
- ASP.NET Core Identity works well for scenarios where storing user information in a cookie is appropriate
- Configure Identity using Entity Framework Core
- Use `AddDefaultIdentity<IdentityUser>()` or `AddIdentityApiEndpoints<IdentityUser>()`
- Enable with `app.UseAuthentication()` and `app.UseAuthorization()` - order matters
- UserManager type for creating users, SignInManager for authentication
- Identity middleware reads cookies to identify users on subsequent requests

**Relevance:** Provides architectural guidance on using Identity in microservices and confirms cookie-based authentication pattern.

### [Share authentication cookies among ASP.NET apps](https://learn.microsoft.com/en-us/aspnet/core/security/cookie-sharing?view=aspnetcore-10.0)
**Accessed:** 2025-12-10

**Key Takeaways:**
- When using ASP.NET Core Identity, data protection keys and app name must be shared among apps
- Use `ConfigureApplicationCookie` extension method to set up data protection service for cookies
- Default authentication type is `Identity.Application`
- For security reasons, authentication cookies are not compressed in ASP.NET Core

**Relevance:** Important for understanding cookie configuration with Identity, especially if we need to share cookies across services in the future.

### [Configure ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-10.0)
**Accessed:** 2025-12-10

**Key Takeaways:**
- ISecurityStampValidator regenerates ClaimsPrincipal after security-sensitive actions
- SecurityStampValidator hooks into OnValidatePrincipal event of each cookie
- Validation interval is a tradeoff between hitting the datastore too frequently and not often enough
- Call `userManager.UpdateSecurityStampAsync(user)` to force existing cookies to be invalidated
- Most Identity UI pages call this after changing password or adding a login

**Relevance:** Important for implementing "sign out everywhere" functionality and security stamp validation.

### [Use cookie authentication without ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0)
**Accessed:** 2025-12-10

**Key Takeaways:**
- ASP.NET Core Identity is a complete authentication provider
- Cookie-based authentication can be used without Identity
- Starting with ASP.NET Core 10, known API endpoints return 401/403 instead of redirecting to login pages

**Relevance:** Confirms that Identity is the recommended approach for full-featured authentication, not just cookie middleware alone.

## Web Resources

### [Audit.NET ASP.NET Identity Integration](https://www.nuget.org/packages/Audit.EntityFramework.Identity.Core/)
**Accessed:** 2025-12-10

**Key Takeaways:**
- Audit.EntityFramework.Identity.Core package specifically enables integration with ASP.NET Core Identity
- Targets .NET 6.0 and .NET Standard 2.1, compatible with .NET 9
- Monitors changes made through the ASP.NET Identity DbContext (user registration, role changes, password updates, etc.)
- Automatically generates audit logs for Identity operations
- Configure alongside IdentityDbContext to track entity changes made by Identity operations

**Relevance:** **Critical finding** - Audit.NET has native support for ASP.NET Identity through a dedicated package. This confirms compatibility and provides a clear integration path.

### [Audit.NET Official Documentation](https://thepirat000.github.io/Audit.NET/)
**Accessed:** 2025-12-10

**Key Takeaways:**
- Audit.NET is an extensible framework to audit executing operations in .NET
- Provides packages for ASP.NET Core Web API and MVC (Audit.WebApi.Core, Audit.Mvc.Core)
- Can audit HTTP requests, user actions, and more
- Works with EF Core for database auditing

**Relevance:** Confirms that Audit.NET can audit both HTTP requests and database operations, which we're already using.

### [Configuring Audit.NET and getting user details from ASP.NET Core app](https://github.com/thepirat000/Audit.NET/discussions/368)
**Accessed:** 2025-12-10

**Key Takeaways:**
- Access IHttpContextAccessor to get current HttpContext including authenticated user information
- Can capture user details during audited operations
- Works with ASP.NET Core Identity authentication

**Relevance:** Confirms that our current Audit.NET setup will work with Identity's cookie authentication to capture user information.

## Current System Analysis

### Current Authentication Implementation

**User Entity:**
- Located at `App/Server/src/Server.Core/UserAggregate/User.cs`
- Properties: Email, Username, HashedPassword (marked with `[AuditIgnore]`), Bio, Image
- Custom relationships: Following/Followers through UserFollowing entity
- Inherits from EntityBase (has Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, ChangeCheck)

**Authentication Infrastructure:**
- JWT-based authentication using System.IdentityModel.Tokens.Jwt
- Custom BcryptPasswordHasher for password hashing (BCrypt.Net)
- Custom JwtTokenGenerator for creating JWT tokens
- Token returned in response with user details

**DbContext:**
- AppDbContext inherits from AuditDbContext (Audit.EntityFramework)
- Uses `[AuditDbContext(Mode = AuditOptionMode.OptOut, IncludeEntityObjects = false)]`
- Dispatches domain events after SaveChanges
- Configures EntityBase properties (audit timestamps, row version)

**Endpoints:**
- FastEndpoints used for all API endpoints
- `/api/users` POST for registration
- `/api/users/login` POST for login
- `/api/user` GET for current user
- `/api/user` PUT for update user

### Key Differences from Identity

**Similarities:**
- Entity Framework Core for data access
- Email and password authentication
- User profile data (bio, image)

**Differences:**
- Custom User entity vs IdentityUser
- JWT tokens vs cookies
- Custom password hashing vs Identity's built-in hashing
- Custom endpoints vs MapIdentityApi endpoints
- No roles/claims system (Identity provides this)
- Custom following/followers relationship (not part of Identity)

### Migration Considerations

1. **User Entity:** Need to integrate custom User properties (Bio, Image) and relationships (Following/Followers) with IdentityUser
2. **Authentication Method:** Switch from JWT to cookies
3. **Endpoints:** Replace custom endpoints with MapIdentityApi endpoints
4. **Password Hashing:** Identity uses different password hashing - need migration strategy or keep BCrypt compatibility
5. **Audit.NET:** Use Audit.EntityFramework.Identity.Core package for Identity integration
6. **DbContext:** Need to integrate IdentityDbContext with AuditDbContext
7. **Tests:** Update all tests to use cookie authentication instead of JWT tokens
8. **Frontend:** Update to send cookies instead of Authorization header with JWT

## Key Architecture Decisions

### Decision 1: User Entity Customization Strategy

**Options:**
- **Option A:** Make User inherit from IdentityUser and add custom properties
- **Option B:** Keep User separate and create a separate IdentityUser

**Recommendation:** Option A (inherit from IdentityUser)
- **Pros:** Single user entity, simpler data model, Identity features integrated
- **Cons:** Requires changing existing entity, may need careful migration

### Decision 2: DbContext Integration Strategy

**Options:**
- **Option A:** Create custom DbContext that inherits from both IdentityDbContext and AuditDbContext
- **Option B:** Use IdentityDbContext with Audit.EntityFramework.Identity.Core package

**Recommendation:** Option B (use Identity package)
- **Pros:** Supported pattern, cleaner architecture, uses official Audit.NET Identity integration
- **Cons:** Need to verify full compatibility

### Decision 3: Password Migration Strategy

**Options:**
- **Option A:** Migrate all passwords to Identity's hashing (requires user re-authentication)
- **Option B:** Keep BCrypt for existing users, use Identity hashing for new users
- **Option C:** Start fresh (acceptable per requirements - development phase, no data to migrate)

**Recommendation:** Option C (start fresh)
- **Pros:** Clean start, no migration complexity, aligns with "development phase" requirement
- **Cons:** All users need to re-register (acceptable per requirements)

### Decision 4: Endpoint Strategy

**Options:**
- **Option A:** Keep existing endpoints and adapt them to use Identity
- **Option B:** Replace with MapIdentityApi endpoints entirely

**Recommendation:** Option B (use MapIdentityApi)
- **Pros:** Uses standard Identity endpoints, less custom code, modern approach
- **Cons:** Endpoint paths change (acceptable per requirements - no backward compatibility needed)

## Testing Requirements

### Test Types to Maintain
1. **Integration Tests (xUnit):** Update to use cookie authentication
2. **Postman Tests:** Update to use /register and /login Identity endpoints with cookies
3. **E2E Playwright Tests:** Update to use cookie-based authentication flows

### Key Testing Considerations
- Cookie handling in HttpClient for integration tests
- Postman cookie management
- Playwright cookie persistence across page navigations
- Test data setup with Identity's UserManager

## React SPA Cookie Authentication Implementation

### [Stack Overflow: ASP.NET Core Web API with Identity - React SPA Frontend](https://stackoverflow.com/questions/77681147/asp-net-core-web-api-with-identity-react-spa-frontend-identity-cookies-not-s)
**Accessed:** 2025-12-10

**Key Takeaways:**
- Set `SameSite=None` on cookies for cross-origin requests (development scenario)
- Set `Cookie.SecurePolicy = CookieSecurePolicy.Always` for secure cookies
- Configure CORS to allow credentials from React app's origin
- Example: `builder.WithOrigins("http://localhost:3000").AllowCredentials()`
- Suppress redirects to login pages for API endpoints - return 401 instead
- React must use `credentials: 'include'` in fetch or `withCredentials: true` in axios

**Relevance:** Critical for our React SPA integration - provides exact configuration needed for cross-origin cookie authentication.

### [Auth0: Authenticate Single-Page Apps With Cookies](https://auth0.com/docs/manage-users/cookies/spa-authenticate-with-cookies)
**Accessed:** 2025-12-10

**Key Takeaways:**
- Cookie-based authentication is recommended for browser-based SPAs
- Browsers automatically handle cookies without exposing them to JavaScript (reduces XSS risk)
- All subsequent requests from React include the cookie automatically
- Proper CORS configuration is essential for cross-origin scenarios

**Relevance:** Confirms that cookie-based authentication is the recommended approach for React SPAs from a security perspective.

### [Andrew Lock: Making authenticated cross-origin requests with ASP.NET Core Identity](https://andrewlock.net/making-authenticated-cross-origin-requests-with-aspnetcore-identity/)
**Accessed:** 2025-12-10

**Key Takeaways:**
- Deep dive into CORS configuration with cookie authentication
- Origin mismatch is a common issue between backend and frontend
- HTTPS requirement when using `SameSite=None`
- Browser behavior varies with `SameSite=None` cookies over HTTP

**Relevance:** Provides troubleshooting guidance for cross-origin cookie authentication scenarios.

### [GitHub Example: login-registration-react-js-web-api-asp-net-core-identity-authentication](https://github.com/aqyanoos/login-registration-react-js-web-api-asp-net-core-identity-authentication)
**Accessed:** 2025-12-10

**Key Takeaways:**
- Full working example of React + ASP.NET Core Identity with cookie authentication
- Demonstrates proper cookie configuration
- Shows React fetch implementation with `credentials: 'include'`

**Relevance:** Reference implementation we can study for our migration.

### React Fetch Configuration

**From Microsoft Learn and Web Resources:**
```javascript
// React fetch with cookies
fetch('https://localhost:5001/api/data', {
  method: 'GET',
  credentials: 'include', // Critical for sending cookies
  headers: {
    'Content-Type': 'application/json',
  },
});

// Axios alternative
axios.get('https://localhost:5001/api/data', {
  withCredentials: true, // Critical for cookies
});
```

**Relevance:** This is the exact pattern our React client needs to implement.

### CORS Configuration for React SPA

**From Microsoft Learn and Web Resources:**
```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowReactSPA", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // React dev server
               .AllowCredentials()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Cookie configuration
services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None; // Allow cross-origin
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401; // Return 401 instead of redirect
        return Task.CompletedTask;
    };
});
```

**Relevance:** This is the exact backend configuration our ASP.NET Core app needs.

## CSRF/Antiforgery Protection

### [Microsoft Learn: Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0)
**Accessed:** 2025-12-10

**Key Takeaways:**
- CSRF attacks are possible when using cookie authentication
- Antiforgery tokens provide protection for state-changing requests
- For SPAs, expose an endpoint to get the antiforgery token
- Token should be sent in request header (e.g., `X-CSRF-TOKEN`)
- Configure with `builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN")`
- Cookie should have `HttpOnly = false` so JavaScript can read it
- Only validate tokens for unsafe HTTP methods (POST, PUT, DELETE)

**Relevance:** Critical security consideration for cookie-based authentication in our SPA.

### CSRF Implementation Pattern for React SPA

**From Microsoft Learn:**
```csharp
// Backend: Endpoint to get antiforgery token
app.MapGet("antiforgery/token", (IAntiforgery forgeryService, HttpContext context) =>
{
    var tokens = forgeryService.GetAndStoreTokens(context);
    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
            new CookieOptions { HttpOnly = false });
    return Results.Ok();
}).RequireAuthorization();

// Configuration
builder.Services.AddAntiforgery(options => {
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.HttpOnly = false; // Allow JS to read
});
```

```javascript
// Frontend: Fetch token and include in requests
fetch('/antiforgery/token').then(() => {
  const token = document.cookie.match(/XSRF-TOKEN=([^;]+)/)[1];
  fetch('/api/protected-endpoint', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-CSRF-TOKEN': token,
    },
    credentials: 'include',
    body: JSON.stringify(data)
  });
});
```

**Relevance:** This pattern will be needed for our React client to make secure state-changing requests.

### [Duende: Understanding Anti-Forgery in ASP.NET Core](https://duendesoftware.com/blog/20250325-understanding-antiforgery-in-aspnetcore)
**Accessed:** 2025-12-10

**Key Takeaways:**
- Comprehensive explanation of how antiforgery works
- Token consists of two parts: cookie and header/form field
- Both must match for validation to succeed
- Important for any cookie-based authentication system

**Relevance:** Helps understand the underlying mechanism for implementing CSRF protection correctly.

### CSRF Considerations

**Key Points from Research:**
1. **When CSRF Protection is Needed:**
   - POST, PUT, DELETE requests (state-changing operations)
   - When using cookie authentication
   - For endpoints that modify server state

2. **When CSRF Protection is NOT Needed:**
   - GET requests (read-only)
   - Bearer token authentication (JWT)
   - Requests with `application/json` content and CORS enabled
   - The `/login` and `/register` endpoints from MapIdentityApi (these are designed to work without antiforgery)

3. **Implementation Strategy:**
   - Create `/antiforgery/token` endpoint for React to fetch token
   - Configure antiforgery to read from `X-CSRF-TOKEN` header
   - React fetches token on app load and includes it in state-changing requests
   - Apply `[ValidateAntiForgeryToken]` or `[AutoValidateAntiforgeryToken]` to endpoints

## Summary

This migration will transition from a custom JWT-based authentication system to ASP.NET Core Identity with cookie-based authentication. The key findings are:

1. **Identity API Endpoints:** Use `MapIdentityApi<TUser>()` for API-only authentication (no UI required)
2. **Cookie Authentication:** Use `AddIdentityCookies()` for browser-based cookie authentication
3. **React SPA Integration:** 
   - Use `credentials: 'include'` in all fetch calls
   - Configure CORS with `AllowCredentials()` for React origin
   - Set cookies to `SameSite=None; Secure` for cross-origin
   - Return 401 status instead of redirects for API endpoints
4. **CSRF Protection:**
   - Implement antiforgery token endpoint for React to fetch
   - Include `X-CSRF-TOKEN` header in state-changing requests
   - MapIdentityApi endpoints handle their own CSRF protection
5. **Custom User Properties:** Extend IdentityUser to include Bio, Image, and Following relationships
6. **Audit.NET Compatibility:** Confirmed - use Audit.EntityFramework.Identity.Core package
7. **DbContext Integration:** Use IdentityDbContext<TUser> with Audit.NET's Identity package
8. **No Backward Compatibility:** Per requirements, we don't need to maintain existing endpoints or data
9. **Testing:** All three test types (Integration, Postman, E2E) need updates for cookie authentication

**Critical Implementation Details:**
- Frontend must include `credentials: 'include'` in every API call
- Backend must configure CORS to allow credentials from frontend origin
- Cookie configuration must use `SameSite=None` for development (different origins)
- Antiforgery tokens needed for POST/PUT/DELETE operations
- Identity's MapIdentityApi endpoints work with cookies by using `?useCookies=true` query parameter

The migration will be executed in multiple phases to maintain a working system after each phase.
