## phase_2

### Phase Overview

Configure ASP.NET Identity services with appropriate security policies and cookie authentication settings. Update the IUserContext implementation to work with Identity's cookie-based claims. This phase registers Identity services and configures authentication but does not yet expose Identity endpoints or change any existing endpoints.

### Prerequisites

- Phase 1 completed: Identity infrastructure added, ApplicationUser created, DbContext updated, migrations applied
- All tests still passing with old authentication system
- Application builds and runs successfully

### Implementation Steps

1. **Add Identity Services in ServiceConfigs.cs**
   - Open `Server.Web/Configurations/ServiceConfigs.cs`
   - After the existing JWT authentication configuration, add Identity services:
     ```csharp
     services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
     {
         // Password policy (Decision 5: Balanced Security Policy)
         options.Password.RequireDigit = true;
         options.Password.RequireLowercase = true;
         options.Password.RequireUppercase = true;
         options.Password.RequireNonAlphanumeric = false;
         options.Password.RequiredLength = 8;
         
         // Lockout policy
         options.Lockout.MaxFailedAccessAttempts = 5;
         options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
         options.Lockout.AllowedForNewUsers = true;
         
         // User options
         options.User.RequireUniqueEmail = true;
         
         // Sign-in options (no email confirmation for now)
         options.SignIn.RequireConfirmedEmail = false;
         options.SignIn.RequireConfirmedPhoneNumber = false;
     })
     .AddEntityFrameworkStores<AppDbContext>()
     .AddDefaultTokenProviders();
     ```

2. **Configure Cookie Authentication**
   - After AddIdentity in ServiceConfigs.cs, configure cookie settings:
     ```csharp
     services.ConfigureApplicationCookie(options =>
     {
         // Cookie settings (Decision 4: SameSite.Lax)
         options.Cookie.HttpOnly = true;
         options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
         options.Cookie.SameSite = SameSiteMode.Lax;
         options.Cookie.Name = "ConduitAuth";
         
         // Expiration and sliding
         options.ExpireTimeSpan = TimeSpan.FromDays(7);
         options.SlidingExpiration = true;
         
         // API-friendly: Return 401/403 instead of redirecting
         options.Events.OnRedirectToLogin = context =>
         {
             context.Response.StatusCode = StatusCodes.Status401Unauthorized;
             return Task.CompletedTask;
         };
         options.Events.OnRedirectToAccessDenied = context =>
         {
             context.Response.StatusCode = StatusCodes.Status403Forbidden;
             return Task.CompletedTask;
         };
     });
     ```

3. **Update IUserContext Implementation for Cookie Authentication**
   - Open `Server.Infrastructure/Services/UserContext.cs`
   - The existing implementation extracts claims from HttpContext.User
   - Verify it works with Identity's cookie claims (should be compatible)
   - Identity uses the same ClaimTypes (NameIdentifier, Email, Name)
   - Test that GetCurrentUserId(), GetRequiredCurrentUserId(), IsAuthenticated() work correctly
   - Note: GetCurrentToken() method may need adjustment - for now it can return empty string or be marked obsolete since cookies don't expose tokens to code the same way

4. **Verify Service Registration Order**
   - Ensure AddIdentity is called before ConfigureApplicationCookie
   - Ensure both JWT authentication (existing) and Identity cookie authentication are registered
   - At this point, both authentication schemes are available but not yet mapped to endpoints

5. **Verify Middleware Pipeline Order**
   - Open `Server.Web/Program.cs` or `MiddlewareConfig.cs`
   - Confirm middleware order is correct:
     1. UseRouting()
     2. UseCors()
     3. UseAuthentication()
     4. UseAuthorization()
     5. UseFastEndpoints()
   - This order ensures both authentication schemes can be evaluated

6. **Build and Test**
   - Run `./build.sh BuildServer` to ensure compilation succeeds
   - Run `./build.sh TestServer` to verify tests still pass
   - Start the application and verify it starts without errors
   - Old authentication endpoints should still work (JWT-based)
   - Identity endpoints are not yet available (not mapped)

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass. The application should build successfully with both authentication systems configured. Old JWT authentication endpoints continue to work. All existing tests pass.