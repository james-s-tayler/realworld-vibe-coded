## phase_17

### Phase Overview

Decommission the legacy /api/users/register and /api/users/login endpoints. Remove JWT token generation services and authentication middleware configuration. Clean up the codebase to use only Identity endpoints. This completes the migration to ASP.NET Identity.

### Prerequisites

- Phase 16 completed: Frontend and E2E tests using cookie authentication
- All tests passing with Identity endpoints
- Old /api/users endpoints unused by all tests and frontend

### Implementation Steps

1. **Remove Register Endpoint**
   - Delete `App/Server/src/Server.Web/Users/Register/` folder completely
   - This includes:
     - Register.cs (endpoint)
     - RegisterRequest.cs
     - RegisterResponse.cs
     - RegisterValidator.cs
     - UserData.cs
   - Delete `App/Server/src/Server.UseCases/Users/Register/` folder
   - This includes:
     - RegisterUserCommand.cs
     - RegisterUserHandler.cs

2. **Remove Login Endpoint**
   - Delete `App/Server/src/Server.Web/Users/Login/` folder completely
   - This includes:
     - Login.cs (endpoint)
     - LoginRequest.cs
     - LoginResponse.cs
     - LoginValidator.cs
     - UserData.cs (if different from Register)
   - Delete `App/Server/src/Server.UseCases/Users/Login/` folder (if exists)
   - This includes:
     - LoginUserCommand.cs (if exists)
     - LoginUserHandler.cs (if exists)

3. **Remove JWT Token Services**
   - Delete `App/Server/src/Server.UseCases/Interfaces/IJwtTokenGenerator.cs`
   - Delete `App/Server/src/Server.Infrastructure/Authentication/JwtTokenGenerator.cs`
   - Open `App/Server/src/Server.Web/Configurations/ServiceConfigs.cs`
   - Remove service registrations:
     ```csharp
     // Remove: services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
     ```

4. **Remove JWT Authentication Middleware**
   - Open `App/Server/src/Server.Web/Configurations/ServiceConfigs.cs`
   - Remove JWT Bearer authentication configuration:
     - Remove `.AddJwtBearer("Token", ...)` call
     - Update default authentication scheme if needed
     - Keep Identity cookie authentication
   - Remove JWT settings configuration binding:
     ```csharp
     // Remove: var jwtSettings = builder.Configuration.GetSection("JwtSettings");
     ```

5. **Remove JWT Settings from Configuration**
   - Open `App/Server/src/Server.Web/appsettings.json`
   - Remove `JwtSettings` section
   - Open `App/Server/src/Server.Web/appsettings.Development.json`
   - Remove `JwtSettings` section
   - Delete JWT settings class file if it exists

6. **Remove JWT-Related NuGet Packages**
   - Open `App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj`
   - Remove package reference (if still present):
     ```xml
     <!-- Remove: <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="..." /> -->
     ```
   - Open `App/Server/src/Server.Web/Server.Web.csproj`
   - Remove package reference (if present):
     ```xml
     <!-- Remove: <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="..." /> -->
     ```
   - Run `dotnet restore` to update dependencies

7. **Search for Remaining References**
   - Use grep or IDE search to find:
     - "IJwtTokenGenerator"
     - "JwtTokenGenerator"
     - "/api/users/register"
     - "/api/users/login"
     - "Token " (authorization header prefix)
     - "JwtSettings"
   - Remove or update any remaining references

8. **Update GetCurrentUser and UpdateUser Endpoints**
   - Review `/api/user` GET endpoint (GetCurrentUser)
   - Review `/api/user` PUT endpoint (UpdateUser)
   - Ensure they work correctly with Identity's cookie authentication
   - These should already work from phase 4, just verify

9. **Clean Up Comments**
   - Search for migration-related comments:
     - "TODO"
     - "TEMPORARY"
     - "Both Identity and legacy"
   - Remove or update comments that reference old authentication system

10. **Build and Test**
    - Run `./build.sh BuildServer`
    - Verify compilation succeeds
    - Run `./build.sh TestServer`
    - Verify all functional tests pass
    - Run `./build.sh TestServerPostman`
    - Verify all postman tests pass (now using only Identity endpoints)
    - Run `./build.sh TestE2e`
    - Verify all E2E tests pass

11. **Verify Database Schema**
    - Check that no migrations are needed
    - Old User table was removed in phase 4
    - Only AspNetUsers table exists

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintAllVerify
./build.sh BuildServer
./build.sh BuildClient
./build.sh TestServer
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass. Codebase should be clean with only Identity authentication code. No legacy JWT endpoints or services remain. All tests pass using Identity endpoints exclusively. Migration complete.
