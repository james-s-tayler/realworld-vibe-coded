## phase_7

### Phase Overview

Remove all legacy JWT authentication code from the codebase. This includes old authentication endpoints (Register, Login), authentication services (IJwtTokenGenerator, IPasswordHasher), JWT middleware configuration, the old User entity, and related specifications. Clean up NuGet package references. The application should be fully using Identity with no remnants of the old authentication system.

### Prerequisites

- Phase 6 completed: All tests (functional, Postman, E2E) passing with Identity
- No code is using old authentication endpoints or services
- System is fully operational with Identity

### Implementation Steps

1. **Remove Old Authentication Endpoints**
   - Delete `Server.Web/Users/Register/` folder (Register endpoint and handler)
   - Delete `Server.Web/Users/Login/` folder (Login endpoint and handler)
   - Keep GetCurrent and Update endpoints if they still work with Identity
   - Or replace them with Identity equivalents if needed

2. **Remove Old Authentication Services**
   - Delete `Server.UseCases/Interfaces/IJwtTokenGenerator.cs`
   - Delete `Server.Infrastructure/Authentication/JwtTokenGenerator.cs`
   - Delete `Server.UseCases/Interfaces/IPasswordHasher.cs`
   - Delete `Server.Infrastructure/Authentication/BcryptPasswordHasher.cs`
   - Remove service registrations from `ServiceConfigs.cs`:
     - Remove `services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>()`
     - Remove `services.AddScoped<IPasswordHasher, BcryptPasswordHasher>()`

3. **Remove JWT Authentication Middleware Configuration**
   - Open `Server.Web/Configurations/ServiceConfigs.cs`
   - Remove JWT Bearer authentication configuration:
     - Remove `AddAuthentication("Token")` and `.AddJwtBearer()` configuration
     - Remove `JwtSettings` configuration binding
     - Remove custom OnMessageReceived, OnChallenge, OnForbidden events
   - Keep only Identity's cookie authentication configuration

4. **Remove JWT Settings from Configuration**
   - Open `appsettings.json` and `appsettings.Development.json`
   - Remove `JwtSettings` section (Secret, Issuer, Audience, ExpirationInDays)
   - If JwtSettings class exists in code, delete it

5. **Update or Remove User Specifications**
   - Delete authentication-specific specifications (no longer needed):
     - `Server.Core/UserAggregate/Specifications/UserByEmailAndPasswordSpec.cs`
     - `Server.Core/UserAggregate/Specifications/UserByEmailSpec.cs`
     - `Server.Core/UserAggregate/Specifications/UserByUsernameSpec.cs`
   - **Keep and update** relationship specifications (still needed for Following/Followers functionality):
     - Update `UserWithFollowingSpec` to query `ApplicationUser` instead of `User`
     - Update `UserByUsernameWithFollowingSpec` to query `ApplicationUser` instead of `User`
     - Update `IsFollowingSpec` if needed (it queries UserFollowing table, may not need changes)
   - Update all calling code (handlers, mappers) to use the updated specifications
   - Delete `Server.Core/UserAggregate/User.cs` (old entity) after all references are updated

6. **Remove Users DbSet from AppDbContext**
   - Open `Server.Infrastructure/Data/AppDbContext.cs`
   - Remove `DbSet<User> Users` property
   - Remove `UserConfiguration` from OnModelCreating if it was explicitly applied
   - Delete `Server.Infrastructure/Data/Config/UserConfiguration.cs`

7. **Remove JWT-Related NuGet Packages**
   - Open `Server.Infrastructure/Server.Infrastructure.csproj`
   - Remove package references:
     - `Microsoft.AspNetCore.Authentication.JwtBearer`
     - `System.IdentityModel.Tokens.Jwt`
     - `BCrypt.Net-Next`
   - Open `Server.Web/Server.Web.csproj`
   - Remove JWT-related packages if any are there
   - Run `dotnet restore` to update dependencies

8. **Create EF Core Migration to Remove Old User Table**
   - Run `dotnet ef migrations add RemoveOldUserTable -p App/Server/src/Server.Infrastructure -s App/Server/src/Server.Web`
   - Review the migration to ensure it drops the old User table
   - Be careful: verify it doesn't drop ApplicationUser (AspNetUsers) table
   - Note: Migrations are applied automatically on application startup

9. **Remove Migration-Related Comments**
   - Search codebase for comments like "TEMPORARY: Both Identity and legacy JWT"
   - Remove or update comments that reference the old authentication system

10. **Search for Remaining References**
    - Use IDE or grep to search for:
      - "IJwtTokenGenerator"
      - "IPasswordHasher"
      - "JwtTokenGenerator"
      - "BcryptPasswordHasher"
      - "Token " (with space - the Authorization header prefix)
      - "/api/users/login"
      - "/api/users" POST endpoint
    - Ensure no references remain (except in git history or documentation)

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintAllVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostman
./build.sh TestE2e
./build.sh DbMigrationsVerifyAll
```

All targets must pass. The codebase should be clean with no legacy authentication code remaining. Only Identity-based authentication code exists. All tests pass using Identity endpoints exclusively.