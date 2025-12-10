# ASP.NET Core Identity Migration Plan

## Executive Summary

This document outlines a comprehensive plan to migrate the Conduit application's custom authentication system to ASP.NET Core Identity. The migration will leverage ASP.NET Core Identity's built-in API endpoints (`MapIdentityApi`) for authentication while maintaining compatibility with the existing Audit.NET implementation and all current tests.

## Current Architecture

### Authentication Components
- **User Entity**: `Server.Core.UserAggregate.User` - Custom user domain entity
- **Password Hashing**: BCrypt-based custom password hasher (`Server.Infrastructure.Authentication.BcryptPasswordHasher`)
- **JWT Token Generation**: Custom JWT token generator (`Server.Infrastructure.Authentication.JwtTokenGenerator`)
- **Endpoints**: FastEndpoints-based custom authentication endpoints:
  - `POST /api/users/login` - User login
  - `POST /api/users` - User registration
  - `GET /api/user` - Get current user
  - `PUT /api/user` - Update user

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
  - `POST /login` - User login (supports cookie and bearer token auth)
  - `POST /refresh` - Token refresh
  - `GET /confirmEmail` - Email confirmation
  - `POST /resendConfirmationEmail` - Resend confirmation email
  - `POST /forgotPassword` - Password reset request
  - `POST /resetPassword` - Password reset
  - `POST /manage/2fa` - Two-factor authentication
  - `GET /manage/info` - Get user info
  - `POST /manage/info` - Update user info

- **Extensibility**: Can customize `IdentityUser` to add custom properties
- **Password Management**: Built-in password hashing (configurable)
- **Token Management**: Supports JWT bearer tokens and cookie authentication
- **EF Core Integration**: Works seamlessly with Entity Framework Core via `IdentityDbContext`

### Identity Data Model
ASP.NET Core Identity uses several entity types:
- `IdentityUser<TKey>` - User accounts
- `IdentityRole<TKey>` - Roles (optional, not needed for Conduit)
- `IdentityUserClaim` - User claims
- `IdentityUserLogin` - External authentication providers
- `IdentityUserToken` - Authentication tokens
- `IdentityUserRole` - User-role associations (if using roles)

## React Frontend Integration Considerations

### Current Frontend Architecture
The Conduit application uses a React + Vite + TypeScript frontend that communicates with the backend API using JWT bearer tokens. The frontend currently:
- Stores JWT tokens (obtained from login/register endpoints)
- Sends tokens in the `Authorization: Token {jwt}` header format (RealWorld spec uses "Token" prefix instead of "Bearer")
- Manages authentication state via React Context (`AuthContext`)
- Uses a centralized API client (`apiRequest` helper) for all HTTP requests

### Identity API Integration Options for React

Based on research and best practices for ASP.NET Core Identity with React SPAs:

**Option 1: JWT Bearer Tokens (Recommended for Conduit)**
- Identity supports JWT bearer tokens via `AddIdentityApiEndpoints<TUser>()`
- React continues to manage tokens in memory or sessionStorage
- Token sent in `Authorization: Bearer {jwt}` header
- Stateless authentication (no server-side sessions)
- Works well for SPAs and mobile apps
- **Best fit for Conduit**: Maintains current architecture with minimal frontend changes

**Option 2: Cookie-Based Authentication**
- Identity's default authentication mechanism
- More secure (HTTP-only cookies prevent XSS)
- Requires CORS configuration for SPA
- Automatic cookie transmission by browser
- Server-side session management
- **Consideration**: May require significant frontend changes to handle cookie-based auth

**Option 3: Hybrid Approach**
- Use both cookies and JWT tokens
- Cookies for browser-based auth
- JWT for API clients and mobile apps
- Most flexible but more complex

### Recommended Approach: JWT Bearer with Identity

Configure Identity to issue JWT tokens for the React frontend:

```csharp
// In Program.cs
builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>(options =>
    {
        // Configure Identity options
        options.Password.RequiredLength = 6;
        // ... other settings
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add JWT Bearer Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
    };
});
```

### Frontend Changes Required

**Minimal Changes** (using wrapper endpoints):
1. **No API URL changes**: Continue using `/api/users/login`, `/api/users`, etc.
2. **Token format**: Change from `Authorization: Token {jwt}` to `Authorization: Bearer {jwt}` (standard)
3. **Response format**: Ensure wrapper endpoints return RealWorld-compatible responses
4. **Error handling**: May need to adjust for Identity's error response format

**Token Storage Best Practices** (from research):
1. **Do NOT use localStorage**: Vulnerable to XSS attacks
2. **Prefer in-memory storage**: Store token in React state/context
3. **SessionStorage**: Minimally safer than localStorage for dev/testing
4. **HTTP-only cookies**: Most secure option (requires backend changes)

**Example React Integration** (based on research patterns):

```typescript
// API client update
const apiRequest = async (url: string, options: RequestInit = {}) => {
    const token = getAuthToken(); // from context or memory
    
    const headers = {
        'Content-Type': 'application/json',
        ...(token && { Authorization: `Bearer ${token}` }), // Changed from "Token"
        ...options.headers,
    };
    
    const response = await fetch(url, { ...options, headers });
    
    if (response.status === 401) {
        // Handle token expiration
        clearAuthToken();
        // Redirect to login or refresh token
    }
    
    return response;
};

// Login function
const login = async (email: string, password: string) => {
    const response = await apiRequest('/api/users/login', {
        method: 'POST',
        body: JSON.stringify({ user: { email, password } })
    });
    
    if (response.ok) {
        const data = await response.json();
        setAuthToken(data.user.token); // Store in context/memory
        setUser(data.user);
    }
};
```

### CORS Configuration

Since the frontend and backend may run on different ports during development:

```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173") // Vite default port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required if using cookies
    });
});

app.UseCors("AllowReactApp");
```

### Token Refresh Strategy

Identity provides a `/refresh` endpoint for token refresh:

```typescript
// React refresh token handler
const refreshToken = async () => {
    try {
        const response = await apiRequest('/api/identity/refresh', {
            method: 'POST',
            body: JSON.stringify({
                refreshToken: getRefreshToken()
            })
        });
        
        if (response.ok) {
            const data = await response.json();
            setAuthToken(data.accessToken);
            setRefreshToken(data.refreshToken);
            return true;
        }
    } catch (error) {
        // Refresh failed, user needs to re-login
        logout();
    }
    return false;
};

// Automatic token refresh on API calls
const apiRequestWithRefresh = async (url: string, options: RequestInit = {}) => {
    let response = await apiRequest(url, options);
    
    if (response.status === 401) {
        // Try to refresh token
        const refreshed = await refreshToken();
        if (refreshed) {
            // Retry original request with new token
            response = await apiRequest(url, options);
        }
    }
    
    return response;
};
```

### Security Considerations for React SPA

Based on security best practices from research:

1. **XSS Protection**:
   - Sanitize all user inputs
   - Use React's built-in XSS protection (JSX escaping)
   - Avoid `dangerouslySetInnerHTML`
   - Store tokens in memory, not localStorage

2. **Token Expiration**:
   - Set reasonable token lifetimes (e.g., 15 minutes for access, 7 days for refresh)
   - Implement automatic token refresh
   - Clear tokens on logout

3. **HTTPS Only**:
   - Always use HTTPS in production
   - Set `Secure` flag on cookies (if using cookies)
   - Configure HSTS headers

4. **CSRF Protection**:
   - Not needed for JWT bearer tokens
   - Required if using cookie authentication
   - Use anti-forgery tokens

### Testing React Integration

After backend migration, frontend tests should verify:

1. **Authentication Flow**:
   - User can register successfully
   - User can login successfully
   - JWT token is received and stored
   - Token is sent in subsequent requests

2. **Authorization**:
   - Protected routes require authentication
   - Unauthorized users are redirected to login
   - Token expiration is handled gracefully

3. **Error Handling**:
   - Invalid credentials show appropriate errors
   - Network errors are handled
   - Token refresh failures trigger re-login

4. **E2E Tests**:
   - Playwright tests should continue to work
   - May need to update token format expectations
   - Verify full authentication flows

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
   - If needed, rename User table to AspNetUsers (or map ApplicationUser to existing Users table)
   - Add Identity-required fields (PasswordHash, SecurityStamp, etc.)

5. Data Migration Script:
   - Map existing `HashedPassword` to Identity's `PasswordHash` field
   - Set required Identity fields (SecurityStamp, ConcurrencyStamp, etc.)
   - Verify data integrity after migration

**Risks**:
- Existing user passwords (BCrypt) may not be compatible with Identity's default password hasher (PBKDF2)
- Table/column naming conflicts between existing schema and Identity defaults
- Unique constraints may conflict

**Mitigations**:
- Keep existing BCrypt hashed passwords and implement custom `IPasswordHasher<ApplicationUser>` that supports both BCrypt (legacy) and Identity's default hasher (new users)
- Use EF Core's Fluent API to map Identity entities to match existing table/column names
- Carefully review and test migrations in a development environment first

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

2. Replace `IJwtTokenGenerator` usage with Identity's `UserManager<ApplicationUser>` and `SignInManager<ApplicationUser>`

3. Remove custom services:
   - `BcryptPasswordHasher` (or keep for backward compatibility)
   - Custom `JwtTokenGenerator` (or adapt to work with Identity)

4. Update `UserContext` service to work with Identity's claims-based authentication

**Testing Considerations**:
- Verify UserManager can create users successfully
- Confirm password hashing works for both new and existing users
- Test SignInManager authentication flows

### Phase 4: Endpoint Migration
**Goal**: Replace custom FastEndpoints with ASP.NET Core Identity API endpoints

**Current Endpoints**:
- `POST /api/users/login` → `POST /api/users/login` (Identity: `POST /login`)
- `POST /api/users` → `POST /api/users` (Identity: `POST /register`)
- `GET /api/user` → `GET /api/user` (Identity: `GET /manage/info`)
- `PUT /api/user` → `PUT /api/user` (Identity: `POST /manage/info`)

**Migration Approaches**:

**Option A: Use MapIdentityApi with Custom Wrappers** (Recommended)
1. Call `MapIdentityApi<ApplicationUser>()` with custom route prefix:
   ```csharp
   app.MapGroup("/api/identity").MapIdentityApi<ApplicationUser>();
   ```

2. Create wrapper endpoints that match RealWorld API spec:
   ```csharp
   // POST /api/users (register)
   app.MapPost("/api/users", async (RegisterRequest request, UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwt) =>
   {
       var user = new ApplicationUser
       {
           Email = request.User.Email,
           UserName = request.User.Username,
           Bio = "I work at statefarm" // Default
       };
       
       var result = await userManager.CreateAsync(user, request.User.Password);
       if (!result.Succeeded)
       {
           return Results.ValidationProblem(/* map errors */);
       }
       
       var token = jwt.GenerateToken(user);
       return Results.Ok(new UserResponse { User = /* map user */ });
   });
   ```

3. Maintain custom DTOs for RealWorld API compatibility:
   - Keep existing request/response models
   - Map between Identity models and RealWorld models

**Option B: Use Identity Services with Custom Endpoints** (Alternative)
1. Keep existing FastEndpoints structure
2. Replace internal logic to use `UserManager<ApplicationUser>` and `SignInManager<ApplicationUser>`
3. Do not use `MapIdentityApi` at all
4. Example:
   ```csharp
   public class Login : Endpoint<LoginRequest, UserResponse>
   {
       private readonly SignInManager<ApplicationUser> _signInManager;
       private readonly UserManager<ApplicationUser> _userManager;
       private readonly IJwtTokenGenerator _jwtGenerator;
       
       public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
       {
           var user = await _userManager.FindByEmailAsync(req.User.Email);
           if (user == null)
           {
               await SendErrorsAsync(/* ... */);
               return;
           }
           
           var result = await _signInManager.CheckPasswordSignInAsync(user, req.User.Password, false);
           if (!result.Succeeded)
           {
               await SendErrorsAsync(/* ... */);
               return;
           }
           
           var token = _jwtGenerator.GenerateToken(user);
           await SendOkAsync(/* map response */);
       }
   }
   ```

**Recommendation**: Start with **Option A** as it provides:
- Built-in security features from Identity
- Additional endpoints for future features (2FA, password reset, email confirmation)
- Cleaner separation between Identity concerns and domain logic
- Can still maintain RealWorld API compatibility through wrapper endpoints

**JWT Token Considerations**:
- Identity can be configured to return JWT bearer tokens instead of cookies
- Use `AddIdentityApiEndpoints<ApplicationUser>()` which includes JWT support
- Configure JWT settings to match existing token format and claims
- May need to adjust JWT claims mapping to maintain compatibility with existing authorization logic

### Phase 5: Test Migration
**Goal**: Ensure all tests pass with the new Identity-based authentication

**Integration Tests** (`Server.IntegrationTests`):
1. Update test fixtures to use Identity's `UserManager` for user creation
2. Replace direct `AppDbContext` user manipulation with `UserManager` calls
3. Update assertions to work with Identity's user model
4. Verify Audit.NET still captures authentication operations

**Functional Tests** (`Server.FunctionalTests`):
1. Update helper methods that create authenticated clients:
   ```csharp
   // Before
   var (token, user) = await CreateUserAndLogin("username", "email@test.com", "password");
   
   // After (may be similar)
   var (token, user) = await CreateUserAndLoginWithIdentity("username", "email@test.com", "password");
   ```

2. Ensure tests work with new endpoint paths (if changed)
3. Verify JWT tokens work correctly for authentication
4. Update any tests that directly manipulate user passwords

**Postman Tests** (`Test/Postman/`):
1. **Critical Decision**: Do we maintain RealWorld API spec or adopt Identity endpoints?
   - **Option 1**: Keep RealWorld spec by using wrapper endpoints (no Postman changes)
   - **Option 2**: Update Postman collection to use Identity endpoints
   
2. If using Identity endpoints directly:
   - Update request URLs and payloads
   - Adjust response validation
   - Update environment variables if needed

3. Run full Postman suite to verify:
   - Registration works
   - Login returns valid JWT
   - Protected endpoints accept JWT
   - User updates work correctly
   - All authentication flows are functional

**E2E Playwright Tests** (`Test/e2e/E2eTests/`):
1. Review page object models for authentication pages
2. Update if authentication flows change (e.g., different response structures)
3. Ensure all user journeys still work:
   - User registration
   - User login
   - Profile updates
   - Article creation/editing (authenticated actions)
   
4. Verify UI still works with new authentication backend

**Test Execution Strategy**:
1. Start with Integration tests (fastest feedback)
2. Progress to Functional tests
3. Run Postman tests for API contract validation
4. Finally, run E2E tests for full system validation
5. Create a test report comparing before/after migration

**Acceptance Criteria**:
- All Integration tests pass
- All Functional tests pass
- All Postman tests pass (100% success rate)
- All E2E Playwright tests pass
- No regression in authentication functionality
- Audit logs continue to capture authentication events correctly

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

## Risk Assessment & Mitigation

### High Risks

**Risk 1: Password Hash Incompatibility**
- **Impact**: Existing users cannot log in after migration
- **Likelihood**: High
- **Mitigation**:
  - Implement custom `IPasswordHasher<ApplicationUser>` that supports both BCrypt (legacy) and Identity's hasher (new)
  - On first successful login with BCrypt password, rehash with Identity's hasher
  - Thoroughly test with sample of real user data

**Risk 2: Breaking API Contract**
- **Impact**: Frontend application breaks, Postman tests fail
- **Likelihood**: Medium-High
- **Mitigation**:
  - Use wrapper endpoints to maintain RealWorld API spec
  - Version the API if making breaking changes
  - Extensive testing with Postman collection
  - Coordinate with frontend team

**Risk 3: Audit.NET Integration Issues**
- **Impact**: Loss of audit trail, compliance issues
- **Likelihood**: Low-Medium
- **Mitigation**:
  - Test Audit.NET thoroughly in development
  - Validate audit logs match expected format
  - Use `Audit.EntityFramework.Identity` package
  - Keep backup of audit logs during migration

### Medium Risks

**Risk 4: Frontend-Backend Integration Issues**
- **Impact**: React app cannot authenticate, broken user flows
- **Likelihood**: Medium
- **Mitigation**:
  - Use wrapper endpoints to maintain API contract
  - Update token format gradually (support both "Token" and "Bearer" during transition)
  - Comprehensive integration testing between frontend and backend
  - Clear communication between frontend and backend developers
  - Test in development environment before deployment

**Risk 5: Test Suite Failures**
- **Impact**: Delayed migration, unknown regressions
- **Likelihood**: Medium
- **Mitigation**:
  - Update tests incrementally
  - Create new test fixtures for Identity
  - Run tests frequently during migration
  - Fix tests immediately when they fail

**Risk 6: Performance Degradation**
- **Impact**: Slower authentication operations
- **Likelihood**: Low
- **Mitigation**:
  - Benchmark authentication operations before/after
  - Optimize Identity configuration
  - Use appropriate caching strategies

**Risk 7: Data Migration Errors**
- **Impact**: User data corruption or loss
- **Likelihood**: Low
- **Mitigation**:
  - Back up database before migration
  - Test migration scripts on copy of production data
  - Implement data validation checks
  - Plan rollback procedure

### Low Risks

**Risk 8: Missing Identity Features**
- **Impact**: Cannot implement desired features
- **Likelihood**: Very Low
- **Mitigation**:
  - ASP.NET Core Identity is highly extensible
  - Can fall back to custom implementation if needed

## Implementation Timeline

### Estimated Effort
- **Phase 1**: Research & Planning - 1-2 days ✓
- **Phase 2**: Database Schema Migration - 2-3 days
- **Phase 3**: Authentication Service Migration - 2-3 days
- **Phase 4**: Endpoint Migration - 3-4 days
- **Phase 5**: Test Migration - 3-4 days
- **Phase 6**: Authorization & User Context - 1-2 days
- **Phase 7**: Audit.NET Integration Validation - 1-2 days
- **Phase 8**: Frontend Integration - 2-3 days
- **Phase 9**: Documentation & Knowledge Transfer - 1-2 days

**Total Estimated Time**: 16-26 working days (3-5 weeks)

### Recommended Approach
1. **Incremental Migration**: Implement one phase at a time with full testing before moving to next phase
2. **Feature Branch**: Use a long-lived feature branch for the migration
3. **Regular Integration**: Merge main into feature branch regularly to avoid conflicts
4. **Review Checkpoints**: Conduct code reviews after each phase
5. **Rollback Plan**: Maintain ability to rollback at each phase

## Success Criteria

### Technical Success
- [ ] All Integration tests pass (100% success rate)
- [ ] All Functional tests pass (100% success rate)
- [ ] All Postman tests pass (100% success rate)
- [ ] All E2E Playwright tests pass (100% success rate)
- [ ] Frontend can authenticate via new backend endpoints
- [ ] JWT tokens work correctly between frontend and backend
- [ ] Audit.NET continues to log all database operations correctly
- [ ] No sensitive data in audit logs (passwords, tokens, security stamps)
- [ ] Authentication performance is equal or better than before
- [ ] All existing users can log in successfully

### Code Quality
- [ ] Code follows repository conventions and patterns
- [ ] All security best practices followed
- [ ] No hardcoded credentials or secrets
- [ ] Proper error handling and logging
- [ ] Code is well-documented with XML comments
- [ ] All analyzer warnings resolved

### Documentation
- [ ] Migration plan document complete and reviewed
- [ ] Authentication documentation created
- [ ] Audit documentation updated
- [ ] README updated
- [ ] Code comments explain Identity integration
- [ ] Known issues documented

### Business Success
- [ ] No user-facing service interruption
- [ ] All user accounts migrated successfully
- [ ] API contract maintained (or gracefully versioned)
- [ ] Feature parity with existing authentication system
- [ ] Foundation for future authentication features (2FA, OAuth, etc.)

## Rollback Plan

### Pre-Migration Backup
1. Back up production database
2. Tag current codebase in Git
3. Document current configuration
4. Export audit logs

### Rollback Triggers
- More than 5% of users cannot log in
- Critical security vulnerability discovered
- Audit logs not being generated
- Performance degradation > 50%
- Unresolvable bugs in production

### Rollback Procedure
1. Stop application
2. Checkout previous Git tag
3. Restore database from backup
4. Verify authentication works
5. Resume application
6. Investigate issues in development environment

### Post-Rollback Actions
1. Root cause analysis of failure
2. Update migration plan to address issues
3. Additional testing before retry
4. Communication to stakeholders

## Future Enhancements

### After Migration
Once Identity is successfully integrated, these features become easier to implement:

1. **Two-Factor Authentication (2FA)**
   - Identity provides built-in 2FA support
   - Can enable via `POST /manage/2fa` endpoint

2. **Email Confirmation**
   - Identity includes email confirmation flows
   - Endpoints: `GET /confirmEmail`, `POST /resendConfirmationEmail`

3. **Password Reset**
   - Built-in password reset functionality
   - Endpoints: `POST /forgotPassword`, `POST /resetPassword`

4. **External Authentication Providers**
   - Easy integration with Google, GitHub, Microsoft, etc.
   - Uses `IdentityUserLogin` for external provider mapping

5. **Token Refresh**
   - Identity API includes `POST /refresh` endpoint
   - Enables long-lived sessions with refresh tokens

6. **User Lockout**
   - Automatic account lockout after failed login attempts
   - Configurable lockout duration and threshold

7. **Security Stamp Validation**
   - Invalidate tokens when password changes
   - Force re-login on security events

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

### React + ASP.NET Core Identity Integration
1. [React 18 Authentication with .NET 6.0 (ASP.NET Core) JWT API - Jason Watmore](https://jasonwatmore.com/react-18-authentication-with-net-6-aspnet-core-jwt-api)
2. [JWT Auth Best Practices in .NET Core & React - FacileTechnolab](https://www.faciletechnolab.com/blog/best-practices-for-implementing-jwt-auth-in-net-core-and-react/)
3. [React + ASP.NET Core JWT Authentication - GitHub Example](https://github.com/HarinduA/react-dotnet-jwt-auth)
4. [How to use Identity to secure a Web API backend for SPAs - Anuraj](https://anuraj.dev/blog/how-to-use-identity-to-secure-a-web-api-backend-for-spas/)
5. [JWT Authentication using .NET and React - Andreyka26](https://andreyka26.com/jwt-auth-using-dot-net-and-react)
6. [Authentication with React.js and ASP.NET Core - Mahedee.net](https://mahedee.net/authentication-and-authorization-using-react.js-and-asp.net-core/)
7. [ASP.NET Core Identity with JWT and React - RainstormTech](https://www.rainstormtech.com/aspnet-core-identity-with-jwt-and-react/)

### RealWorld API Specification
1. [RealWorld API Spec](https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints)
2. [Conduit API Endpoints](https://github.com/gothinkster/realworld/tree/main/api)

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

### A.3: Program.cs Identity Configuration
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

### A.4: Custom Password Hasher (BCrypt Compatibility)
```csharp
using Microsoft.AspNetCore.Identity;
using BCrypt.Net;

namespace Server.Infrastructure.Authentication;

/// <summary>
/// Custom password hasher that supports both BCrypt (legacy) and Identity's default hasher
/// </summary>
public class HybridPasswordHasher : IPasswordHasher<ApplicationUser>
{
    private readonly PasswordHasher<ApplicationUser> _identityHasher;

    public HybridPasswordHasher()
    {
        _identityHasher = new PasswordHasher<ApplicationUser>();
    }

    public string HashPassword(ApplicationUser user, string password)
    {
        // Use Identity's default hasher for new passwords
        return _identityHasher.HashPassword(user, password);
    }

    public PasswordVerificationResult VerifyHashedPassword(
        ApplicationUser user, 
        string hashedPassword, 
        string providedPassword)
    {
        // Try Identity's default hasher first
        var identityResult = _identityHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        
        if (identityResult == PasswordVerificationResult.Success)
        {
            return identityResult;
        }

        // If Identity verification fails, try BCrypt (legacy)
        try
        {
            if (BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword))
            {
                // Password is correct but uses old BCrypt format
                // Return SuccessRehashNeeded to trigger password rehash
                return PasswordVerificationResult.SuccessRehashNeeded;
            }
        }
        catch
        {
            // BCrypt verification failed or hash format invalid
        }

        return PasswordVerificationResult.Failed;
    }
}

// Register in Program.cs:
// builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, HybridPasswordHasher>();
```

## Appendix B: Migration Checklist

### Pre-Migration
- [ ] Create feature branch for migration
- [ ] Back up production database
- [ ] Review and approve migration plan
- [ ] Set up development environment with test data
- [ ] Notify team of migration start

### Phase 2: Database Schema
- [ ] Install NuGet packages
- [ ] Create ApplicationUser class
- [ ] Update AppDbContext
- [ ] Create EF Core migration
- [ ] Review migration SQL
- [ ] Test migration on development database
- [ ] Create data migration script
- [ ] Test data migration with sample data
- [ ] Verify data integrity

### Phase 3: Authentication Services
- [ ] Configure Identity in Program.cs
- [ ] Implement custom password hasher
- [ ] Update UserContext service
- [ ] Remove/update custom authentication services
- [ ] Test UserManager operations
- [ ] Test SignInManager operations
- [ ] Verify JWT token generation

### Phase 4: Endpoints
- [ ] Map Identity API endpoints
- [ ] Create wrapper endpoints for RealWorld API
- [ ] Test registration endpoint
- [ ] Test login endpoint
- [ ] Test get current user endpoint
- [ ] Test update user endpoint
- [ ] Verify JWT authentication on protected endpoints
- [ ] Test error responses

### Phase 5: Tests
- [ ] Update integration test fixtures
- [ ] Run and fix integration tests
- [ ] Update functional test helpers
- [ ] Run and fix functional tests
- [ ] Update/validate Postman tests
- [ ] Run full Postman suite
- [ ] Update E2E test page objects (if needed)
- [ ] Run and fix E2E tests
- [ ] Create test report

### Phase 6: Authorization
- [ ] Update UserContext implementation
- [ ] Verify JWT claims mapping
- [ ] Test authorization on all protected endpoints
- [ ] Test with invalid tokens
- [ ] Test with expired tokens
- [ ] Verify user context in audit logs

### Phase 7: Audit.NET
- [ ] Verify audit logs for Identity operations
- [ ] Confirm sensitive data excluded
- [ ] Test custom fields capture
- [ ] Validate audit log format
- [ ] Test transaction rollback scenarios
- [ ] Review audit configuration

### Phase 8: Frontend Integration
- [ ] Update API client to use Bearer token format
- [ ] Test registration flow from React
- [ ] Test login flow from React
- [ ] Verify JWT token storage and retrieval
- [ ] Test protected API calls with token
- [ ] Test token expiration handling
- [ ] Update error handling for Identity errors
- [ ] Configure CORS settings
- [ ] Test logout functionality
- [ ] Run React unit tests
- [ ] Manual testing of all auth flows
- [ ] Verify E2E tests still pass

### Phase 9: Documentation
- [ ] Update AUDIT.md
- [ ] Create AUTHENTICATION.md
- [ ] Update README.md
- [ ] Update backend instructions
- [ ] Update frontend instructions (if needed)
- [ ] Create migration notes
- [ ] Document known issues
- [ ] Update API documentation
- [ ] Document React integration patterns

### Validation & Sign-off
- [ ] All tests passing (100%)
- [ ] Code review completed
- [ ] Security review completed
- [ ] Performance benchmarks acceptable
- [ ] Documentation reviewed and approved
- [ ] Rollback plan tested
- [ ] Stakeholder sign-off

### Deployment
- [ ] Deploy to staging environment
- [ ] Run full test suite on staging
- [ ] Performance testing on staging
- [ ] Security scan on staging
- [ ] Deploy to production (off-peak hours)
- [ ] Monitor application logs
- [ ] Monitor audit logs
- [ ] Monitor error rates
- [ ] Verify user logins working
- [ ] 24-hour monitoring period

### Post-Migration
- [ ] Archive old authentication code
- [ ] Clean up unused dependencies
- [ ] Update CI/CD pipelines (if needed)
- [ ] Team training session
- [ ] Post-mortem meeting
- [ ] Document lessons learned
- [ ] Plan future enhancements

---

**Document Version**: 1.1  
**Last Updated**: 2025-12-10  
**Author**: Development Team  
**Status**: Ready for Review  
**Changelog**:
- v1.1: Added React frontend integration considerations, token storage best practices, CORS configuration, and security recommendations
- v1.0: Initial comprehensive migration plan
