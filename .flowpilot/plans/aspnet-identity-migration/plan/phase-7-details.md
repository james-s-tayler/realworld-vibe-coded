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

5. **Remove Old User Entity and Specifications**
   - Delete `Server.Core/UserAggregate/User.cs` (old entity)
   - Delete User specifications:
     - `Server.Core/UserAggregate/Specifications/UserByEmailAndPasswordSpec.cs`
     - `Server.Core/UserAggregate/Specifications/UserByEmailSpec.cs`
     - `Server.Core/UserAggregate/Specifications/UserByUsernameSpec.cs`
     - `Server.Core/UserAggregate/Specifications/UserWithFollowingSpec.cs`
     - `Server.Core/UserAggregate/Specifications/UserByUsernameWithFollowingSpec.cs`
     - `Server.Core/UserAggregate/Specifications/IsFollowingSpec.cs`
   - If any code still references User entity, update to use ApplicationUser

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

8. **Create and Apply EF Core Migration to Remove Old User Table**
   - Run `dotnet ef migrations add RemoveOldUserTable -p App/Server/src/Server.Infrastructure -s App/Server/src/Server.Web`
   - Review the migration to ensure it drops the old User table
   - Be careful: verify it doesn't drop ApplicationUser (AspNetUsers) table
   - Apply migration: `dotnet ef database update -p App/Server/src/Server.Infrastructure -s App/Server/src/Server.Web`

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

11. **Build and Test**
    - Run `./build.sh BuildServer` to ensure compilation succeeds
    - Run `./build.sh TestServer` to verify all tests still pass
    - Run `./build.sh TestServerPostman` to verify Postman tests pass
    - Run `./build.sh TestE2e` to verify E2E tests pass
    - Fix any issues that arise from removals

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintAllVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass. The codebase should be clean with no legacy authentication code remaining. Only Identity-based authentication code exists. All tests pass using Identity endpoints exclusively.