## Current System Analysis

This analysis documents the authentication and authorization components in the Conduit application that will be affected by the migration to ASP.NET Core Identity.

### Authentication Architecture

#### Current Authentication Method: JWT Token-Based

The application currently uses custom JWT token-based authentication with the following components:

**Authentication Scheme**: Custom "Token" scheme using JWT Bearer authentication
- Configuration in: `Server.Web/Configurations/ServiceConfigs.cs`
- Token format: `Authorization: Token <jwt-token>`
- Claims: NameIdentifier (User.Id), Email, Name (Username)

**Token Generation**:
- `Server.Infrastructure/Authentication/JwtTokenGenerator.cs` - Implements `IJwtTokenGenerator`
- Creates JWT tokens with user claims (ID, Email, Username)
- Configured via `JwtSettings` (Secret, Issuer, Audience, ExpirationInDays)
- Token expiration: Configurable days (from appsettings)

**Password Hashing**:
- `Server.Infrastructure/Authentication/BcryptPasswordHasher.cs` - Implements `IPasswordHasher`
- Uses BCrypt.NET for password hashing and verification
- Stored in `User.HashedPassword` field

#### User Context Service

**IUserContext Interface** (`Server.UseCases/Interfaces/IUserContext.cs`):
- `GetCurrentUserId()` - Returns nullable Guid
- `GetRequiredCurrentUserId()` - Throws if not authenticated
- `IsAuthenticated()` - Boolean check
- `GetCurrentToken()` - Extracts JWT from Authorization header
- `GetCurrentUsername()` - Returns username claim
- `GetCorrelationId()` - For audit correlation

**UserContext Implementation** (`Server.Infrastructure/Services/UserContext.cs`):
- Uses `IHttpContextAccessor` to access current HTTP context
- Extracts claims from `HttpContext.User.Claims`
- Parses claims: ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Email

### Domain Model

#### User Entity (`Server.Core/UserAggregate/User.cs`)

**Properties**:
- `Id` (Guid) - Primary key
- `Email` (string, max 255) - Unique identifier
- `Username` (string, 2-100 chars) - Display name, unique
- `HashedPassword` (string, max 255) - BCrypt hashed password, marked with `[AuditIgnore]`
- `Bio` (string, max 1000) - User biography
- `Image` (string?, max 500) - Profile image URL
- `Following` (ICollection<UserFollowing>) - Users this user follows
- `Followers` (ICollection<UserFollowing>) - Users following this user

**Validation Constraints**:
- EmailMaxLength = 255
- UsernameMinLength = 2, UsernameMaxLength = 100
- PasswordMinLength = 6
- HashedPasswordMaxLength = 255
- BioMaxLength = 1000
- ImageUrlMaxLength = 500

**Key Methods**:
- `UpdateEmail()`, `UpdateUsername()`, `UpdatePassword()` - Update user properties
- `Follow()`, `Unfollow()`, `IsFollowing()` - Relationship management

**Important Note**: User entity inherits from `EntityBase` which provides audit fields (`CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, `ChangeCheck`)

### Authentication Endpoints

#### Registration (`Server.Web/Users/Register/Register.cs`)
- **Route**: `POST /api/users`
- **Allows Anonymous**: Yes
- **Request**: RegisterRequest with UserData (username, email, password)
- **Handler**: RegisterUserHandler
- **Flow**:
  1. Validate input (FluentValidation)
  2. Check email uniqueness
  3. Hash password with BCrypt
  4. Create User entity
  5. Save to database
  6. Generate JWT token
  7. Return UserResponse with token

#### Login (`Server.Web/Users/Login/Login.cs`)
- **Route**: `POST /api/users/login`
- **Allows Anonymous**: Yes
- **Request**: LoginRequest with LoginUserData (email, password)
- **Handler**: LoginUserHandler
- **Flow**:
  1. Validate input
  2. Query user by email and password (using specification)
  3. Verify password with BCrypt
  4. Generate JWT token
  5. Return LoginResponse with token

#### Get Current User (`Server.Web/Users/GetCurrent/GetCurrent.cs`)
- **Route**: `GET /api/user`
- **Requires Authentication**: Yes
- **Handler**: GetCurrentUserHandler
- **Flow**:
  1. Extract user ID from claims (via IUserContext)
  2. Query user from database
  3. Return UserCurrentResponse with current JWT token

#### Update User (`Server.Web/Users/Update/UpdateUser.cs`)
- **Route**: `PUT /api/user`
- **Requires Authentication**: Yes
- **Handler**: UpdateUserHandler
- **Flow**:
  1. Extract user ID from claims
  2. Validate input
  3. Update user properties (email, username, password, bio, image)
  4. If password provided, hash with BCrypt
  5. Save changes
  6. Generate new JWT token
  7. Return UpdateUserResponse with new token

### Database Context

#### AppDbContext (`Server.Infrastructure/Data/AppDbContext.cs`)

**Current Implementation**:
- Inherits from `Audit.EntityFramework.AuditDbContext` (not standard `DbContext`)
- Decorated with `[AuditDbContext(Mode = AuditOptionMode.OptOut, IncludeEntityObjects = false)]`
- DbSets: Users, Articles, Tags, Comments, UserFollowings
- SaveChangesAsync overridden for domain event dispatching
- OnModelCreating configures EntityBase properties (ChangeCheck, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)

**Key Configuration**:
- `UserConfiguration` (`Server.Infrastructure/Data/Config/UserConfiguration.cs`) - EF Core entity configuration
- Unique indexes on Email and Username
- Password and audit fields required
- Following/Follower relationships configured

### Audit.NET Integration

**Current Setup**:
- `AppDbContext` inherits from `Audit.EntityFramework.AuditDbContext`
- User.HashedPassword marked with `[AuditIgnore]` attribute
- Audit configuration in `Server.Infrastructure/Data/AuditConfiguration.cs`
- Logs written to `Logs/Server.Web/Audit.NET/` directory
- Both EntityFrameworkEvent and DatabaseTransactionEvent are logged

**Critical for Migration**: Audit.NET's `AuditDbContext` is a subclass of EF Core's `DbContext`. ASP.NET Core Identity also requires a DbContext subclass (IdentityDbContext). This is the key compatibility challenge.

### Testing Infrastructure

#### Functional Tests (`App/Server/tests/Server.FunctionalTests/`)

**Users Tests** (`Users/UsersTests.cs`, `UsersFixture.cs`):
- Uses `WebApplicationFactory<Program>` pattern
- Tests: Registration, Login, GetCurrentUser, UpdateUser
- Creates test users with helper methods
- Uses FastEndpoints test extensions (POSTAsync, GETAsync, PUTAsync)

**Articles Tests** (various files under `Articles/`):
- Many tests require authenticated users
- Uses ArticlesFixture to create authenticated HttpClient instances
- Tests follow AAA pattern (Arrange, Act, Assert)

#### Test Patterns:
- **Fixtures**: Shared setup/teardown logic, creates authenticated clients
- **HttpClient creation**: `CreateClient()` with Authorization header set to JWT token
- **Database cleanup**: Tests use a fresh database or reset between runs

### Middleware and Configuration

#### Middleware Pipeline (`Server.Web/Program.cs`, `Server.Web/Configurations/MiddlewareConfig.cs`):
- CORS policy: "AllowLocalhost" (allows any origin/header/method)
- Authentication middleware: `app.UseAuthentication()`
- Authorization middleware: `app.UseAuthorization()`
- FastEndpoints: `app.UseFastEndpoints()`
- SPA fallback routing for non-API routes

#### Service Registration (`Server.Web/Configurations/ServiceConfigs.cs`):
- JWT authentication configured with "Token" scheme
- Custom `OnMessageReceived` event to extract token from "Token " prefix
- Custom `OnChallenge` and `OnForbidden` events for error formatting
- Services: IUserContext → UserContext, IJwtTokenGenerator → JwtTokenGenerator, IPasswordHasher → BcryptPasswordHasher

### Specifications (Ardalis.Specification)

**User Specifications** (`Server.Core/UserAggregate/Specifications/`):
- `UserByEmailAndPasswordSpec` - Used for login
- `UserByEmailSpec` - Check email uniqueness
- `UserByUsernameSpec` - Check username uniqueness
- `UserWithFollowingSpec` - Load user with following relationships
- `UserByUsernameWithFollowingSpec` - Combined query
- `IsFollowingSpec` - Check following relationship

These will need to continue working after Identity migration or be replaced with Identity patterns.

### Dependencies

**Authentication-Related NuGet Packages**:
- `Microsoft.AspNetCore.Authentication.JwtBearer` - Current JWT auth
- `BCrypt.Net-Next` - Password hashing
- `System.IdentityModel.Tokens.Jwt` - JWT token generation
- `Audit.EntityFramework.Core` - EF Core auditing

**To Add**:
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` - Identity with EF Core
- `Audit.EntityFramework.Identity` - Audit.NET support for Identity (if available)

### Cross-Cutting Concerns

#### Logging (Serilog):
- Configured in `Program.cs`
- Logs to console and file
- Used in authentication handlers for debugging

#### Validation (FluentValidation):
- All mutating endpoints have validators
- `LoginValidator`, `RegisterValidator`, `UpdateUserValidator`
- Integrated with FastEndpoints

#### Error Handling:
- Custom error formatting in JWT events
- FastEndpoints `SendErrorsAsync()` for consistent error responses
- Result pattern used in handlers (Result<T>)

### Key Observations for Migration

1. **JWT vs Cookie**: Current system uses JWT tokens with "Token " prefix. Need to switch to cookie-based authentication.
2. **User Entity Compatibility**: User entity has custom fields (Bio, Image, Following/Followers) that must be preserved.
3. **Audit.NET DbContext Conflict**: Both Audit.NET and Identity require DbContext subclassing - need resolution strategy.
4. **IUserContext Abstraction**: Good abstraction layer that isolates auth details from use cases - should be maintained.
5. **Specification Pattern**: User queries use specifications - these may need adjustment for Identity.
6. **Test Infrastructure**: Extensive test suite must continue to work with cookie auth instead of JWT.
7. **FastEndpoints Integration**: All endpoints use FastEndpoints, not controllers - Identity endpoints also need FastEndpoints.
8. **Password Requirements**: Current minimum is 6 characters; Identity defaults may differ.
9. **Token in Responses**: Current endpoints return JWT token in response body - need to transition to cookie-only.
10. **Clean Architecture**: Strong separation between layers must be maintained.