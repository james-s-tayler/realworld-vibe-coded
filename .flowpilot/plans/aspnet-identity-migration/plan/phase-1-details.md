## phase_1

### Phase Overview

Add ASP.NET Identity infrastructure to the application without breaking existing functionality. This phase installs required packages, creates the ApplicationUser entity, updates the DbContext to support both Audit.NET and Identity, and applies the database migration. The old authentication system remains fully functional.

### Prerequisites

- All existing tests pass (functional, Postman, E2E)
- Application builds and runs successfully
- Database is accessible and migrations are up to date

### Implementation Steps

1. **Install Required NuGet Packages**
   - Add `Microsoft.AspNetCore.Identity.EntityFrameworkCore` to Server.Infrastructure project
   - Add `Audit.EntityFramework.Identity` to Server.Infrastructure project
   - Run `dotnet restore` to ensure packages are installed

2. **Create ApplicationUser Entity**
   - Create `Server.Core/IdentityAggregate/ApplicationUser.cs` class
   - Extend `IdentityUser<Guid>` to use Guid as primary key (matching existing User entity)
   - Add custom properties: `Bio` (string, max 1000), `Image` (string?, max 500)
   - Add navigation properties for Following/Followers relationships:
     - `ICollection<UserFollowing> Following`
     - `ICollection<UserFollowing> Followers`
   - Mark sensitive fields with `[AuditIgnore]` attribute if needed (e.g., PasswordHash is already handled by Identity)
   - Follow the same validation constraints as the existing User entity

3. **Create ApplicationUser Configuration**
   - Create `Server.Infrastructure/Data/Config/ApplicationUserConfiguration.cs`
   - Implement `IEntityTypeConfiguration<ApplicationUser>`
   - Configure property constraints (max lengths for Bio, Image)
   - Configure Following/Followers relationships with proper foreign keys
   - This configuration will be applied when the DbContext is updated

4. **Update AppDbContext**
   - Change base class from `Audit.EntityFramework.AuditDbContext` to `Audit.EntityFramework.Identity.AuditIdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
   - Add using statements: `using Microsoft.AspNetCore.Identity;`, `using Audit.EntityFramework.Identity;`
   - Keep all existing DbSets (Users, Articles, Tags, Comments, UserFollowings)
   - Keep existing SaveChangesAsync override for domain event dispatching
   - Keep existing OnModelCreating configuration
   - In OnModelCreating, apply ApplicationUserConfiguration: `builder.ApplyConfiguration(new ApplicationUserConfiguration());`
   - Note: Both old User table and new AspNetUsers tables will coexist

5. **Create Entity Framework Migration**
   - Run `dotnet ef migrations add AddIdentityTables -p App/Server/src/Server.Infrastructure -s App/Server/src/Server.Web`
   - Review the migration file to ensure it adds Identity tables (AspNetUsers, AspNetRoles, AspNetUserClaims, etc.)
   - Verify the migration includes ApplicationUser custom properties (Bio, Image)
   - Verify Following/Followers relationship tables are configured correctly
   - Note: Migrations are applied automatically on application startup

6. **Build and Run Application**
   - Run `./build.sh BuildServer` to ensure compilation succeeds
   - Run `./build.sh TestServer` to verify existing tests still pass
   - Start the application and verify it runs without errors
   - Existing authentication endpoints should still work (don't test Identity yet)

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostman
./build.sh DbMigrationsVerifyAll
```

All targets must pass with no errors. The application should build successfully, all tests should pass, and the old authentication system should remain fully functional.