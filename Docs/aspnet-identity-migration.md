# ASP.NET Core Identity Migration Plan

## Executive Summary

This document outlines a comprehensive plan to migrate the Conduit application's custom authentication system to ASP.NET Core Identity. The migration will leverage ASP.NET Core Identity's built-in API endpoints (`MapIdentityApi`) using cookie-based authentication for a more secure implementation.

**Important**: This migration explicitly deviates from the RealWorld API specification in favor of using ASP.NET Core Identity's standard endpoints and authentication patterns.


## Current Architecture

### Authentication Components
- **User Entity**: `Server.Core.UserAggregate.User` - Custom user domain entity (to be migrated to `ApplicationUser`)
- **Password Hashing**: ASP.NET Core Identity's password hasher (replacing BCrypt)
- **Authentication**: Cookie-based authentication via ASP.NET Core Identity
- **Endpoints**: ASP.NET Core Identity API endpoints:
  - `POST /register` - User registration
  - `POST /login?useCookies=true` - User login with cookie auth
  - `GET /manage/info` - Get current user
  - `POST /manage/info` - Update user

### Data Model
- **Database**: SQLite with Entity Framework Core
- **User Fields**:
  - `Id` (Guid, Primary Key)
  - `Email` (string, unique index, max 255)
  - `Username` (string, unique index, 2-100 chars)
  - `HashedPassword` (string, max 255, BCrypt hash)
  - `Bio` (string, max 1000)
  - `Image` (string?, max 500)
  - Following/Followers relationships
- **Note**: Since the application is in development and starts from a blank database each time, there are no data migration compatibility concerns.

### Audit Implementation
- **Library**: Audit.NET with Audit.EntityFramework.Core
- **DbContext**: `AppDbContext` inherits from `AuditDbContext`
- **Mode**: Opt-out (audits all entities by default)
- **Sensitive Data**: `HashedPassword` marked with `[AuditIgnore]`
- **Storage**: JSON files in `Logs/Audit/` directory
- **Custom Fields**: Captures `UserId` and `Username` from authenticated user context

### Testing Structure
- **Integration Tests**: `Server.IntegrationTests` - Tests against in-memory database
- **Functional Tests**: `Server.FunctionalTests` - Tests using `WebApplicationFactory`
- **Postman Tests**: Comprehensive API contract tests in `Test/Postman/`
- **E2E Tests**: Playwright tests in `Test/e2e/E2eTests/`

## ASP.NET Core Identity Overview

### Key Features
- **Identity API Endpoints**: `MapIdentityApi<TUser>()` provides REST API endpoints:
  - `POST /register` - User registration
  - `POST /login?useCookies=true` - User login with cookie authentication
  - `GET /manage/info` - Get user info
  - `POST /manage/info` - Update user info
  - `GET /confirmEmail` - Email confirmation
  - `POST /resendConfirmationEmail` - Resend confirmation email
  - `POST /forgotPassword` - Password reset request
  - `POST /resetPassword` - Password reset
  - `POST /manage/2fa` - Two-factor authentication
  - `GET /manage/info` - Get user info
  - `POST /manage/info` - Update user info

- **Extensibility**: Can customize `IdentityUser` to add custom properties
- **Password Management**: Built-in secure password hashing
- **Cookie Authentication**: HTTP-only cookies for secure, SPA-compatible authentication
- **EF Core Integration**: Works seamlessly with Entity Framework Core via `IdentityDbContext`

### Identity Data Model
ASP.NET Core Identity uses several entity types:
- `IdentityUser<TKey>` - User accounts
- `IdentityRole<TKey>` - Roles (optional, not needed for Conduit)
- `IdentityUserClaim` - User claims
- `IdentityUserLogin` - External authentication providers
- `IdentityUserToken` - Authentication tokens
- `IdentityUserRole` - User-role associations (if using roles)

## Cookie-Based Authentication with ASP.NET Core Identity

### Why Cookies Over JWT Tokens

For this single-domain SPA application, cookie-based authentication is the recommended approach:

1. **More Secure**: HTTP-only cookies prevent XSS attacks
2. **Built-in Support**: ASP.NET Core Identity provides excellent cookie support for SPAs  
3. **Automatic Management**: Browser automatically sends cookies with requests
4. **No Token Storage Issues**: No need to manage token storage in JavaScript

### Using Identity Endpoints with Cookies

Call Identity endpoints with the `useCookies=true` query parameter:

**Login**: `POST /login?useCookies=true`
**Registration**: `POST /register`  
**Get User Info**: `GET /manage/info`
**Update User**: `POST /manage/info`

All requests must include `credentials: 'include'` to send cookies.

## Audit.NET and ASP.NET Core Identity Compatibility

### Compatibility Confirmation
According to research and documentation:
1. **Compatible**: Audit.NET provides `AuditIdentityDbContext` specifically for ASP.NET Core Identity
2. **Package**: `Audit.EntityFramework.Identity` is available for Identity integration
3. **Inheritance**: Can inherit from `AuditIdentityDbContext<TUser>` instead of `IdentityDbContext<TUser>`
4. **Functionality**: All Audit.NET features work with Identity entities

### Implementation Approach
```csharp
// Current
public class AppDbContext : AuditDbContext { }

// After migration (Option 1 - Recommended)
public class AppDbContext : AuditIdentityDbContext<ApplicationUser> { }

// After migration (Option 2 - If not using roles)
public class AppDbContext : AuditIdentityDbContext<ApplicationUser, IdentityRole, string> { }
```

### Custom User Model
```csharp
public class ApplicationUser : IdentityUser<Guid>
{
    // Conduit-specific properties
    public string Bio { get; set; } = default!;
    public string? Image { get; set; }
    
    // Navigation properties for following relationships
    public ICollection<UserFollowing> Following { get; set; } = new List<UserFollowing>();
    public ICollection<UserFollowing> Followers { get; set; } = new List<UserFollowing>();
}
```

### Audit Configuration Considerations
- **Password Hashing**: Identity's `PasswordHash` should be marked with `[AuditIgnore]`
- **Security Tokens**: Identity generates various tokens that should be excluded from audits
- **IncludeEntityObjects**: Keep as `false` to prevent sensitive data logging
- **Custom Fields**: Continue capturing `UserId` and `Username` from authenticated context

## Migration Strategy

### Phase 1: Research & Planning ✓
- [x] Review ASP.NET Core Identity documentation
- [x] Confirm Audit.NET compatibility
- [x] Analyze current implementation
- [x] Create migration plan document

### Phase 2: Database Schema Migration
**Goal**: Extend existing User entity to be compatible with ASP.NET Core Identity

**Steps**:
1. Install required NuGet packages:
   - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
   - `Audit.EntityFramework.Identity` (for Audit.NET compatibility)

2. Create `ApplicationUser` class extending `IdentityUser<Guid>`:
   - Map existing fields (Email, Username) to Identity properties
   - Preserve Conduit-specific fields (Bio, Image)
   - Maintain Following/Followers relationships

3. Update `AppDbContext`:
   - Change inheritance from `AuditDbContext` to `AuditIdentityDbContext<ApplicationUser>`
   - Configure Identity entities in `OnModelCreating`
   - Ensure existing User table structure is preserved/mapped correctly

4. Create and apply EF Core migrations:
   - Add Identity tables (AspNetUserClaims, AspNetUserLogins, AspNetUserTokens, etc.)
   - Map ApplicationUser to existing Users table or create new AspNetUsers table
   - Add Identity-required fields (PasswordHash, SecurityStamp, etc.)
   - **Run**: `nuke DbMigrationsVerifyAll` to verify migrations are correct

**Note**: Since the application is in development and always starts from a blank database, there are no data migration compatibility concerns. All users will be created fresh using Identity's password hashing.

### Phase 3: Authentication Service Migration
**Goal**: Replace custom authentication services with ASP.NET Core Identity services

**Steps**:
1. Configure Identity in `Program.cs`:
   ```csharp
   builder.Services
       .AddIdentityCore<ApplicationUser>(options =>
       {
           // Configure password requirements to match current or less restrictive
           options.Password.RequireDigit = false;
           options.Password.RequiredLength = 6;
           options.Password.RequireNonAlphanumeric = false;
           options.Password.RequireUppercase = false;
           options.Password.RequireLowercase = false;
           
           // Configure user requirements
           options.User.RequireUniqueEmail = true;
           
           // Configure sign-in options
           options.SignIn.RequireConfirmedEmail = false;
           options.SignIn.RequireConfirmedAccount = false;
       })
       .AddEntityFrameworkStores<AppDbContext>()
       .AddSignInManager<SignInManager<ApplicationUser>>()
       .AddDefaultTokenProviders();
   ```

2. Remove custom authentication services:
   - Remove `BcryptPasswordHasher`
   - Remove custom `JwtTokenGenerator`
   - Remove custom authentication endpoints

3. Update `UserContext` service to work with Identity's claims-based authentication

**Testing**:
- Run `nuke TestServer` to verify backend tests pass
- Run `nuke TestServerPostman` to verify API contract tests pass  
- Run `nuke TestE2e` to verify end-to-end tests pass
- Fix any failing tests iteratively

### Phase 4: Endpoint and Test Migration (Combined)
**Goal**: Migrate to ASP.NET Core Identity endpoints and update all tests to pass

**Important**: This phase combines endpoint migration with test updates because tests must pass before merging. We are explicitly deviating from the RealWorld API specification to use ASP.NET Core Identity's standard endpoints.

**Endpoint Migration**:

Use ASP.NET Core Identity endpoints directly (no wrappers):
- `POST /register` - User registration
- `POST /login?useCookies=true` - User login with cookie auth
- `GET /manage/info` - Get current user info
- `POST /manage/info` - Update user info

**Implementation**:
```csharp
// In Program.cs
builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>(options =>
    {
        // Configure Identity options
        options.Password.RequiredLength = 6;
        // ... other settings
    })
    .AddEntityFrameworkStores<AppDbContext>();

// Map Identity API endpoints
app.MapIdentityApi<ApplicationUser>();
```

**Frontend Updates**:
1. Update API client to use cookie authentication (credentials: 'include')
2. Update endpoint URLs to Identity endpoints
3. Remove JWT token management code
4. Update AuthContext to work with cookie-based auth

**Test Migration**:

**Integration Tests** (`Server.IntegrationTests`):
1. Update test fixtures to use Identity's `UserManager` for user creation
2. Replace direct `AppDbContext` user manipulation with `UserManager` calls  
3. Update assertions to work with Identity's user model
4. Update cookie-based authentication in test helpers
5. **Run**: `nuke TestServer` and fix any failing tests iteratively

**Functional Tests** (`Server.FunctionalTests`):
1. Update helper methods that create authenticated clients for cookie auth
2. Update endpoint URLs to use Identity endpoints
3. Update request/response models to match Identity's format
4. **Run**: `nuke TestServer` and fix any failing tests iteratively

**Postman Tests** (`Test/Postman/`):
1. Update Postman collection to use Identity endpoints
2. Update requests to use cookie authentication  
3. Update environment variables if needed
4. **Run**: `nuke TestServerPostman` and fix any failing tests iteratively

**E2E Playwright Tests** (`Test/e2e/E2eTests/`):
1. Update page object models for new authentication endpoints
2. Update authentication flows to work with cookies
3. Ensure all user journeys still work
4. **Run**: `nuke TestE2e` and fix any failing tests iteratively

**Iterative Debugging Process**:

When tests fail, follow this systematic approach:

1. **Read error messages carefully**: Error messages often contain specific guidance on what failed
2. **Check test reports**: Reports are generated in `Reports/Server/Artifacts/Tests/Report.md`
3. **Review logs**: Check application logs if tests are failing due to runtime errors
4. **Search for solutions**: Use mslearn MCP server to search for Identity-specific issues
5. **Web search**: If mslearn doesn't have the answer, use web search for community solutions
6. **Iterate**: Make small fixes and re-run tests until all pass
7. **Don't give up early**: This is a complex migration; persist through multiple iterations

**Required Test Commands**:
```bash
# Run all backend tests
nuke TestServer

# Run Postman API tests  
nuke TestServerPostman

# Run E2E Playwright tests
nuke TestE2e
```

**Acceptance Criteria**: All three test suites must pass (100% success rate) before proceeding.

### Phase 6: Authorization & User Context
**Goal**: Ensure authorization and user context extraction work with Identity's claims

**Steps**:
1. Update `UserContext` service:
   - Change from custom claims to Identity's standard claims
   - Map `ClaimTypes.NameIdentifier` to user ID
   - Map `ClaimTypes.Email` to user email
   - Map `ClaimTypes.Name` to username

2. Verify JWT token claims match existing authorization logic:
   ```csharp
   // Existing JWT generation creates these claims:
   new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
   new Claim(ClaimTypes.Email, user.Email),
   new Claim(ClaimTypes.Name, user.Username)
   
   // Ensure Identity generates compatible claims
   ```

3. Test authorization on protected endpoints:
   - Articles (create, update, delete)
   - Comments (create, delete)
   - User profile updates
   - Following/unfollowing users

4. Update any custom authorization policies if needed

### Phase 7: Audit.NET Integration Validation
**Goal**: Confirm Audit.NET works correctly with Identity entities

**Steps**:
1. Verify audit logs for Identity operations:
   - User registration (Insert to AspNetUsers)
   - User login (if persisting login events)
   - Password changes
   - User profile updates

2. Confirm sensitive data is excluded:
   - `PasswordHash` not in audit logs
   - Security stamps not in audit logs
   - Tokens not in audit logs

3. Test custom fields still captured:
   - `UserId` from authenticated context
   - `Username` from claims

4. Validate audit log format matches expectations:
   - JSON structure unchanged
   - Custom fields present
   - Entity names correct (may change if table names change)

5. Test transaction rollback scenarios:
   - Ensure audit logs reflect failed operations correctly

**Configuration Updates**:
```csharp
[AuditDbContext(
    Mode = AuditOptionMode.OptOut,
    IncludeEntityObjects = false,
    AuditEventType = "{context}")]
public class AppDbContext : AuditIdentityDbContext<ApplicationUser>
{
    // Configure audit to ignore sensitive Identity properties
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Mark Identity's PasswordHash as ignored
        builder.Entity<ApplicationUser>()
            .Property(u => u.PasswordHash)
            .HasAnnotation("AuditIgnore", true);
    }
}
```

### Phase 8: Frontend Integration
**Goal**: Update React frontend to work with Identity-based authentication

**Steps**:
1. **Update API client to use Bearer token format**:
   - Change from `Authorization: Token {jwt}` to `Authorization: Bearer {jwt}`
   - Maintain backward compatibility during transition if needed

2. **Test authentication flows in React**:
   - Registration creates users via new endpoints
   - Login returns valid JWT token
   - Token is stored securely (in-memory or sessionStorage for dev)
   - Token is sent with all authenticated requests
   - Token expiration is handled gracefully

3. **Update AuthContext** (if needed):
   - Adjust for any changes in token structure
   - Implement token refresh logic if using refresh tokens
   - Handle Identity-specific error responses

4. **CORS configuration verification**:
   - Ensure backend CORS policy allows frontend origin
   - Test credentials and headers are properly configured
   - Verify preflight requests work correctly

5. **Security improvements** (optional but recommended):
   - Move from sessionStorage to in-memory token storage
   - Implement automatic token refresh
   - Add token expiration checks before requests

6. **Frontend testing**:
   - Run React unit tests
   - Run React integration tests
   - Manual testing of all authentication flows
   - Verify error handling works correctly

**Testing Checklist**:
- [ ] User can register with valid credentials
- [ ] User receives validation errors for invalid registration
- [ ] User can login with correct credentials
- [ ] Login fails with incorrect credentials
- [ ] JWT token is received and stored
- [ ] Token is sent in Authorization header
- [ ] Protected API calls work with valid token
- [ ] Protected API calls fail with invalid/expired token
- [ ] User can update profile
- [ ] User can logout (token cleared)
- [ ] Page refresh maintains authentication state (if using sessionStorage)

**Coordination with Backend**:
- Ensure wrapper endpoints return RealWorld-compatible responses
- Verify error message formats match frontend expectations
- Confirm token format and claims are correct
- Test end-to-end integration between React and ASP.NET Core

### Phase 9: Documentation & Knowledge Transfer
**Goal**: Document changes and provide guidance for future development

**Deliverables**:
1. Update `Docs/AUDIT.md`:
   - Add section on Identity integration
   - Document which Identity fields are audited
   - Explain audit log differences for Identity entities

2. Create `Docs/AUTHENTICATION.md`:
   - Document new authentication architecture
   - Explain Identity endpoints and usage
   - Provide examples of common authentication tasks
   - Document custom password hasher (if implemented)

3. Update `README.md`:
   - Note migration to ASP.NET Core Identity
   - Link to authentication documentation
   - Update any authentication-related sections

4. Update backend instructions (`.github/instructions/backend.instructions.md`):
   - Add Identity-specific guidance
   - Document how to work with UserManager and SignInManager
   - Provide examples of authentication in tests

5. Create migration notes:
   - Document breaking changes (if any)
   - Provide rollback procedure
   - List known issues or limitations






## References & Resources

### Official Documentation
1. [ASP.NET Core Identity Overview](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
2. [Identity model customization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model)
3. [MapIdentityApi Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.routing.identityapiendpointroutebuilderextensions.mapidentityapi)
4. [Configure JWT bearer authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication)
5. [How to use Identity to secure a Web API backend for SPAs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization)

### Audit.NET Resources
1. [Audit.NET GitHub Repository](https://github.com/thepirat000/Audit.NET)
2. [Audit.EntityFramework Documentation](https://github.com/thepirat000/Audit.NET/blob/master/src/Audit.EntityFramework/README.md)
3. [Audit.EntityFramework.Identity Package](https://www.nuget.org/packages/Audit.EntityFramework.Identity)
4. [Audit.NET with ASP.NET Identity Integration Guide](https://github.com/thepirat000/Audit.NET/discussions/368)

### Migration Examples
1. [Migrate Authentication and Identity to ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/migration/fx-to-core/examples/identity)
2. [Secure ASP.NET Core Blazor WebAssembly with ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-identity)

### Cookie-Based Authentication Resources
1. [How to use Identity to secure a Web API backend for SPAs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization)
2. [Secure ASP.NET Core Blazor WebAssembly with ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-identity)



## Appendix A: Key Code Changes

### A.1: ApplicationUser Definition
```csharp
using Microsoft.AspNetCore.Identity;
using Audit.EntityFramework;

namespace Server.Core.UserAggregate;

public class ApplicationUser : IdentityUser<Guid>
{
    // Constants from existing User entity
    public const int UsernameMinLength = 2;
    public const int UsernameMaxLength = 100;
    public const int PasswordMinLength = 6;
    public const int BioMaxLength = 1000;
    public const int ImageUrlMaxLength = 500;

    // Conduit-specific properties
    public string Bio { get; set; } = "I work at statefarm"; // Default bio
    public string? Image { get; set; }

    // Navigation properties for following relationships
    public ICollection<UserFollowing> Following { get; set; } = new List<UserFollowing>();
    public ICollection<UserFollowing> Followers { get; set; } = new List<UserFollowing>();

    // Override to ensure PasswordHash is not audited
    [AuditIgnore]
    public override string? PasswordHash { get; set; }
    
    // Also ignore security-sensitive fields
    [AuditIgnore]
    public override string? SecurityStamp { get; set; }
    
    [AuditIgnore]
    public override string? ConcurrencyStamp { get; set; }
}
```

### A.2: AppDbContext Update
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Audit.EntityFramework.Identity;

namespace Server.Infrastructure.Data;

[AuditDbContext(
    Mode = AuditOptionMode.OptOut,
    IncludeEntityObjects = false,
    AuditEventType = "{context}")]
public class AppDbContext : AuditIdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            // Map to existing Users table (if keeping same table name)
            entity.ToTable("Users");
            
            // Configure custom properties
            entity.Property(u => u.Bio)
                .HasMaxLength(ApplicationUser.BioMaxLength)
                .IsRequired();
            
            entity.Property(u => u.Image)
                .HasMaxLength(ApplicationUser.ImageUrlMaxLength);
            
            // Configure Identity properties to match existing constraints
            entity.Property(u => u.Email)
                .HasMaxLength(255)
                .IsRequired();
            
            entity.Property(u => u.UserName)
                .HasMaxLength(ApplicationUser.UsernameMaxLength)
                .IsRequired();
        });

        // Configure other entities as before
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

### A.3: Program.cs Identity Configuration with Cookie Authentication
```csharp
using Microsoft.AspNetCore.Identity;
using Server.Core.UserAggregate;
using Server.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ... existing configuration ...

// Add Identity
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // Password settings
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = ApplicationUser.PasswordMinLength;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        
        // User settings
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = 
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        
        // Sign-in settings
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        
        // Lockout settings (optional)
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager<SignInManager<ApplicationUser>>()
    .AddUserManager<UserManager<ApplicationUser>>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication (keep existing settings)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // ... existing JWT configuration ...
    });

var app = builder.Build();

// Map Identity API endpoints
app.MapGroup("/api/identity").MapIdentityApi<ApplicationUser>();

// Map custom wrapper endpoints for RealWorld API compatibility
app.MapPost("/api/users", async (/* ... */) => { /* register wrapper */ });
app.MapPost("/api/users/login", async (/* ... */) => { /* login wrapper */ });
app.MapGet("/api/user", async (/* ... */) => { /* get current user */ }).RequireAuthorization();
app.MapPut("/api/user", async (/* ... */) => { /* update user */ }).RequireAuthorization();

// ... rest of app configuration ...

app.Run();
```


---

**Document Version**: 2.0
**Last Updated**: 2025-12-10
**Author**: Development Team
**Status**: Ready for Implementation
**Changelog**:
- v2.0: Major update - switched to cookie-based authentication, removed JWT/wrappers, using Identity endpoints directly, removed data migration concerns, combined Phase 4 and 5, added specific Nuke commands, removed Risk/Timeline/Rollback/Future sections
- v1.1: Added React frontend integration considerations
- v1.0: Initial comprehensive migration plan
