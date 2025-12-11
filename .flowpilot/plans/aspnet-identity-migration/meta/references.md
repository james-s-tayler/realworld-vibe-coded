# References for ASP.NET Identity Migration

## Official Microsoft Documentation

### [Introduction to Identity on ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-10.0)
**Key Takeaways:**
- ASP.NET Core Identity is an API that supports user interface (UI) login functionality and manages users, passwords, profile data, roles, claims, tokens, email confirmation
- Identity is typically configured using a database (Entity Framework Core) to store user names, passwords, and profile data
- The Identity source code is available on GitHub for reference
- Templates treat username and email as the same for users

**Relevance:** Provides foundation understanding of what ASP.NET Core Identity is and how it differs from custom authentication implementations.

### [How to use Identity to secure a Web API backend for SPAs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0)
**Key Takeaways:**
- `MapIdentityApi<TUser>()` adds JSON API endpoints for registering, logging in, and managing users
- The endpoints include: `/register`, `/login`, `/refresh`, `/confirmEmail`, `/resendConfirmationEmail`, `/forgotPassword`, `/resetPassword`, `/manage/2fa`, `/manage/info`
- Cookie-based authentication is the recommended approach for browser-based applications
- Custom logout endpoint can be implemented using `SignInManager<TUser>`
- Since ASP.NET Core 10, API endpoints return 401/403 status codes instead of redirecting to login pages

**Relevance:** Critical for understanding the built-in Identity API endpoints that will replace our custom authentication endpoints. Shows how to use cookie authentication with MapIdentityApi.

### [Use cookie authentication without ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0)
**Key Takeaways:**
- Cookie authentication can be configured independently of Identity if needed
- Important cookie settings: HttpOnly, Secure, SameSite, ExpireTimeSpan, SlidingExpiration
- UseAuthentication() middleware must be called after UseRouting() but before UseAuthorization()
- API endpoints with cookie auth return 401/403 instead of redirect (ASP.NET Core 10+)

**Relevance:** Provides understanding of cookie authentication configuration and best practices, essential for our cookie-based authentication requirement.

### [IdentityDbContext Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identitydbcontext?view=aspnetcore-10.0)
**Key Takeaways:**
- IdentityDbContext is the base class for Entity Framework database context used for Identity
- It's generic and supports customization of entity types
- Multiple overloads available for different levels of customization
- Can be used with or without roles

**Relevance:** Critical for understanding how Identity integrates with Entity Framework Core and how it might interact with Audit.NET's DbContext subclassing.

### [Identity model customization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-10.0)
**Key Takeaways:**
- Identity provides default entity types: IdentityUser, IdentityRole, IdentityUserClaim, IdentityUserToken, IdentityUserLogin, IdentityRoleClaim, IdentityUserRole
- Can customize the primary key data type (e.g., use Guid instead of string)
- Custom properties can be added to ApplicationUser by extending IdentityUser
- Identity options affecting EF Core model must be applied at design time for migrations

**Relevance:** Shows how to customize Identity to fit our application needs while maintaining best practices.

### [Configure ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-10.0)
**Key Takeaways:**
- ConfigureApplicationCookie must be called after AddIdentity or AddDefaultIdentity
- Identity options include password requirements, lockout settings, user options, sign-in options
- Cookie settings can be customized through ConfigureApplicationCookie

**Relevance:** Provides guidance on configuring Identity options to meet security requirements and user experience needs.

### [Share authentication cookies among ASP.NET apps](https://learn.microsoft.com/en-us/aspnet/core/security/cookie-sharing?view=aspnetcore-10.0)
**Key Takeaways:**
- Data protection keys must be shared for cookie sharing across apps
- Use PersistKeysToFileSystem and SetApplicationName for shared cookies
- ConfigureApplicationCookie can customize cookie name and other settings
- Authentication cookies are not compressed for security reasons

**Relevance:** Important if we need to share authentication state across multiple applications or services in the future.

### [Migrate Authentication and Identity to ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/migration/fx-to-core/examples/identity?view=aspnetcore-10.0)
**Key Takeaways:**
- Migration from custom authentication requires updating DbContext, service registration, and middleware
- AddDefaultIdentity or AddIdentity must be called in service configuration
- UseAuthentication() middleware must be added in correct order
- Custom properties and methods must be migrated from old user classes

**Relevance:** Provides a migration pathway from existing authentication systems to ASP.NET Core Identity.

### [Migrate ASP.NET Framework Authentication to ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/migration/fx-to-core/areas/authentication?view=aspnetcore-10.0)
**Key Takeaways:**
- Three migration strategies: complete rewrite, remote authentication, shared cookie authentication
- Complete rewrite offers best performance and maintainability
- Forms authentication → Cookie authentication
- Custom authentication → Custom authentication handlers or middleware
- When not preserving data, clean slate migration is much simpler

**Relevance:** Confirms our approach of not preserving existing data simplifies the migration significantly.

### [Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0)
**Key Takeaways:**
- WebApplicationFactory pattern for integration testing
- Can mock authentication using TestAuthHandler
- ConfigureTestServices allows test-specific service configuration
- Tests follow Arrange-Act-Assert pattern
- Microsoft.AspNetCore.Mvc.Testing package streamlines test creation

**Relevance:** Provides patterns for maintaining integration test parity during the migration, addressing the testing requirement.

### [Authentication and authorization in Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-10.0)
**Key Takeaways:**
- Minimal APIs support all authentication and authorization options
- RequireAuthorization() extension method secures endpoints
- AllowAnonymous() allows unauthenticated access
- Authentication handlers implement strategies for generating user claims
- Both role-based and claims-based authorization supported

**Relevance:** Shows how Identity integrates with minimal APIs and FastEndpoints patterns used in the application.

## Audit.NET Compatibility Research

### [Audit.NET GitHub Repository](https://github.com/thepirat000/Audit.NET)
**Key Takeaways:**
- Audit.NET provides Audit.EntityFramework.Core and Audit.EntityFramework.Identity packages
- AuditIdentityDbContext can be used as base class for Identity contexts
- AuditSaveChangesInterceptor enables auditing without changing base class
- Supports tracking of user management actions, role assignments, password changes, sign-in attempts
- Compatible with .NET 6 and above

**Relevance:** Confirms that Audit.NET has specific support for ASP.NET Core Identity through dedicated packages and patterns.

### Audit.NET Integration Approaches (Web Search)
**Key Takeaways:**
- Two integration patterns: subclass AuditIdentityDbContext or use AuditSaveChangesInterceptor
- Can capture actor (user performing action), timestamp, changed data (old vs new), custom fields
- Flexible storage options (database, file, cloud)
- Performance overhead should be considered and tested
- Configuration can include/exclude specific entity objects

**Relevance:** Provides concrete implementation patterns for maintaining Audit.NET compatibility with Identity's DbContext requirements.

## Cookie Authentication Best Practices (2024)

### Cookie Security Best Practices (Web Search)
**Key Takeaways:**
- Always set HttpOnly = true and SecurePolicy = Always
- Use SameSite = Strict for CSRF prevention (or Lax if cross-site scenarios needed)
- Set reasonable expiration (20-30 minutes) with SlidingExpiration = true
- Require confirmed email for login
- Enable account lockout (MaxFailedAccessAttempts = 5, DefaultLockoutTimeSpan = 10 minutes)
- Require strong passwords (length ≥ 12, require non-alphanumeric)
- Handle 401/403 responses gracefully in clients (no redirect for APIs)

**Relevance:** Ensures our cookie-based authentication implementation follows current security best practices.

## Testing Strategy

### Testing Patterns for Identity (Microsoft Docs + Community)
**Key Takeaways:**
- Use WebApplicationFactory for integration tests
- Mock SignInManager and UserManager for unit tests
- TestAuthHandler pattern for authentication testing
- In-memory database or SQLite for functional tests
- Follow AAA (Arrange-Act-Assert) pattern

**Relevance:** Provides patterns for maintaining test parity (Integration, Postman, E2E) during migration.

## Migration Strategy Without Data Preservation

### Clean Slate Migration Approach (Web Search + Microsoft Docs)
**Key Takeaways:**
- No need to migrate password hashes, user IDs, or claims when not preserving data
- Remove old custom authentication code completely
- Create new ApplicationUser and ApplicationDbContext classes
- Run EF migrations to create Identity tables (AspNetUsers, AspNetRoles, etc.)
- Users must re-register with new system
- Significantly simpler than data-preserving migrations
- No need for complex data mapping or password hash conversion

**Relevance:** Confirms the goal requirement of not maintaining data compatibility simplifies the migration significantly and is a valid approach for development-phase applications.

## Key Decision Points from Research

1. **No UI Dependency Confirmed**: MapIdentityApi provides JSON endpoints without requiring Razor Pages UI
2. **Endpoint Switch Supported**: Identity endpoints are designed to replace custom authentication endpoints
3. **Testing Parity Achievable**: WebApplicationFactory and mocking patterns support all test types
4. **Audit.NET Compatible**: Dedicated packages and patterns exist for Identity + Audit.NET integration
5. **Cookie Auth Supported**: Identity has first-class cookie authentication support for browser-based scenarios
6. **Clean Migration Path**: Not preserving data significantly simplifies the migration process