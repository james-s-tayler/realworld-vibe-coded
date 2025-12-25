## phase_4: Add Organization Entity and EF Core Multi-Tenant Infrastructure

### Phase Overview

Add Organization entity to represent tenants. Migrate AppDbContext from AuditIdentityDbContext to MultiTenantIdentityDbContext inheritance. Configure Audit.NET to work with new DbContext using data provider approach. Add TenantId column to ApplicationUser with foreign key to Organizations table.

**Scope Size:** Small (~10 steps)
**Risk Level:** Medium (DbContext inheritance change is critical)
**Estimated Complexity:** Medium

### Prerequisites

What must be completed before starting this phase:
- Phase 3 completed (all endpoints require authentication)
- All tests passing with authenticated access
- Phase 1 POC validated that MultiTenantIdentityDbContext + Audit.NET work together

### Known Risks & Mitigations

**Risk 1:** DbContext inheritance change may break existing code
- **Likelihood:** Medium
- **Impact:** High (application won't start if DbContext fails)
- **Mitigation:** Follow POC pattern from phase 1 exactly. Test immediately after changing inheritance.
- **Fallback:** Revert to AuditIdentityDbContext, investigate POC findings, run `flowpilot stuck`

**Risk 2:** Audit.NET may not capture events correctly with new DbContext
- **Likelihood:** Low (POC validated this)
- **Impact:** High (compliance requirement)
- **Mitigation:** Verify audit logs after implementation. Check both EntityFrameworkEvent and DatabaseTransactionEvent logs.
- **Fallback:** Add custom audit event enricher if automatic capture fails

**Risk 3:** EF migrations may fail or create incorrect schema
- **Likelihood:** Low
- **Impact:** Medium (database schema issues)
- **Mitigation:** Review migration files before applying. Test with fresh database first.
- **Fallback:** Remove migration, adjust entity configurations, regenerate

### Implementation Steps

**Part 1: Install Finbuckle Package**

1. **Add Finbuckle.MultiTenant NuGet package**
   - Add package reference to Server.Infrastructure project: `Finbuckle.MultiTenant.EntityFrameworkCore` version 10.0+
   - Run `dotnet restore`
   - Expected outcome: Package installed without conflicts
   - Files affected: `App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj`
   - Reality check: Build succeeds, no package conflicts

**Part 2: Create Organization Entity**

2. **Create Organization entity class**
   - Create `Organization.cs` in `App/Server/src/Server.Core/Entities/`
   - Inherit from `EntityBase` to get Id, audit fields, change tracking
   - Add properties: `Name` (string), `Identifier` (string, for Finbuckle)
   - Expected outcome: Organization entity defined
   - Files affected: `App/Server/src/Server.Core/Entities/Organization.cs` (new)
   - Reality check: Code compiles

3. **Create Organization entity configuration**
   - Create `OrganizationConfiguration.cs` in `App/Server/src/Server.Infrastructure/Data/Config/`
   - Implement `IEntityTypeConfiguration<Organization>`
   - Configure required properties, max lengths, unique index on Identifier
   - Example: `builder.Property(o => o.Identifier).IsRequired().HasMaxLength(50); builder.HasIndex(o => o.Identifier).IsUnique();`
   - Expected outcome: Organization table configuration ready
   - Files affected: `App/Server/src/Server.Infrastructure/Data/Config/OrganizationConfiguration.cs` (new)
   - Reality check: Code compiles

**Part 3: Update ApplicationUser Entity**

4. **Add TenantId to ApplicationUser**
   - Open `App/Server/src/Server.Core/Entities/ApplicationUser.cs`
   - Add property: `public Guid? TenantId { get; set; }`
   - Add navigation: `public Organization? Organization { get; set; }`
   - Expected outcome: ApplicationUser has TenantId foreign key
   - Files affected: `App/Server/src/Server.Core/Entities/ApplicationUser.cs`
   - Reality check: Code compiles

5. **Configure ApplicationUser TenantId relationship**
   - Open or create `ApplicationUserConfiguration.cs` in `App/Server/src/Server.Infrastructure/Data/Config/`
   - Configure foreign key: `builder.HasOne(u => u.Organization).WithMany().HasForeignKey(u => u.TenantId).OnDelete(DeleteBehavior.Restrict);`
   - Add index on TenantId: `builder.HasIndex(u => u.TenantId);`
   - Expected outcome: Relationship configured
   - Files affected: `App/Server/src/Server.Infrastructure/Data/Config/ApplicationUserConfiguration.cs`
   - Reality check: Code compiles

**Part 4: Update AppDbContext**

6. **Change AppDbContext inheritance to MultiTenantIdentityDbContext**
   - Open `App/Server/src/Server.Infrastructure/Data/AppDbContext.cs`
   - Change base class from `AuditIdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` to `MultiTenantIdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
   - Remove AuditIdentityDbContext using statement, add Finbuckle using
   - Expected outcome: AppDbContext inherits from MultiTenantIdentityDbContext
   - Files affected: `App/Server/src/Server.Infrastructure/Data/AppDbContext.cs`
   - Reality check: Code compiles

7. **Configure Audit.NET with data provider**
   - In AppDbContext, override `SaveChangesAsync`
   - Configure Audit.EntityFramework data provider (follow POC pattern from phase 1)
   - Add custom field to include TenantId from `IMultiTenantContextAccessor<TenantInfo>` (if available, null for now)
   - Call `base.SaveChangesAsync()` to leverage Finbuckle's tenant association logic
   - Expected outcome: Audit.NET configured via data provider
   - Files affected: `App/Server/src/Server.Infrastructure/Data/AppDbContext.cs`
   - Reality check: Build succeeds, SaveChangesAsync compiles

8. **Add Organization DbSet**
   - In AppDbContext, add: `public DbSet<Organization> Organizations => Set<Organization>();`
   - Expected outcome: Organizations accessible via DbContext
   - Files affected: `App/Server/src/Server.Infrastructure/Data/AppDbContext.cs`
   - Reality check: Build succeeds

**Part 5: Create and Apply EF Migration**

9. **Create EF Core migration and regenerate idempotent script**
   - Run: `dotnet ef migrations add AddOrganizationAndTenantId --project App/Server/src/Server.Infrastructure --startup-project App/Server/src/Server.Web`
   - Regenerate idempotent script: `dotnet ef migrations script --idempotent --output App/Server/src/Server.Infrastructure/Data/Migrations/idempotent.sql --project App/Server/src/Server.Infrastructure --startup-project App/Server/src/Server.Web`
   - Review migration file to verify Organizations table and ApplicationUser.TenantId column are added
   - Verify indexes created on TenantId and Identifier
   - Expected outcome: Migration file and idempotent script created
   - Files affected: `App/Server/src/Server.Infrastructure/Data/Migrations/*_AddOrganizationAndTenantId.cs` (new), `App/Server/src/Server.Infrastructure/Data/Migrations/idempotent.sql` (updated)
   - Reality check: Migration file looks correct (no unexpected changes), run `./build.sh DbMigrationsVerifyIdempotentScript` and `./build.sh DbMigrationsVerifyAll` to verify

10. **Verify migrations**
    - Migrations run automatically on app startup
    - Run: `./build.sh DbMigrationsVerifyAll` to verify migrations apply successfully
    - Verify Organizations table exists and ApplicationUser has TenantId column
    - Expected outcome: Database schema updated, all migration verifications pass
    - Reality check: DbMigrationsVerifyAll target passes

**Part 6: Update Functional Tests**

11. **Update test fixtures to create Organizations**
    - Modify test fixtures (ArticlesFixture, ProfilesFixture, etc.) to create Organization before creating test users
    - Associate test users with Organization by setting TenantId
    - Expected outcome: Test users have TenantId set
    - Files affected: `App/Server/tests/Server.FunctionalTests/Fixtures/*.cs`
    - Reality check: Test setup compiles

12. **Run functional tests**
    - Run: `./build.sh TestServer`
    - Fix any tests that fail due to Organization requirement
    - Expected outcome: All functional tests pass
    - Reality check: 45 functional tests pass

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After package install
./build.sh BuildServer

# After entity changes
./build.sh LintServerVerify
./build.sh BuildServer

# After DbContext changes
./build.sh BuildServer
# Check for compilation errors

# After migration and idempotent script
./build.sh DbMigrationsVerifyIdempotentScript
./build.sh DbMigrationsVerifyAll
# Verify migrations apply successfully

# After test updates
./build.sh TestServer
```

Don't wait until the end to test. Reality test after each major change.

### Expected Working State After Phase

When this phase is complete:
- Organization entity exists in domain
- ApplicationUser has TenantId foreign key to Organization
- AppDbContext inherits from MultiTenantIdentityDbContext
- Audit.NET configured via data provider, still captures events
- EF Core migrations applied successfully
- Database has Organizations table and ApplicationUser.TenantId column
- Indexes created on TenantId columns
- Functional tests pass with Organization creation in fixtures
- Application still works as single-tenant (multi-tenant infrastructure in place but not enforced yet)
- **No query filters active yet** (added in phase 5)

### If Phase Fails

If this phase fails and cannot be completed:
1. If DbContext inheritance fails, review phase 1 POC - compare implementation
2. Use mslearn MCP server to search for MultiTenantIdentityDbContext configuration examples
3. If Audit.NET stops working, check SaveChangesAsync - verify data provider configuration
4. Check Audit.NET logs in `Logs/` directory (e.g., `Logs/Test/e2e`, `Logs/Test/Postman`, `Logs/RunLocal`) to confirm events still captured
5. If migration fails, review entity configurations - ensure foreign keys correct
6. Use debug-analysis.md for complex issues
7. If stuck, run `flowpilot stuck`

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh DbMigrationsVerifyAll
./build.sh TestServer
```

All targets must pass before proceeding to the next phase.

**Manual Verification Steps:**
1. Inspect database schema - verify Organizations table and ApplicationUser.TenantId column exist
2. Check indexes: `SELECT * FROM sqlite_master WHERE type='index' AND tbl_name IN ('Organizations', 'AspNetUsers');`
3. Check Audit.NET logs in `Logs/` directory (specific subfolder depends on which executable generates the logs, e.g., `Logs/Test/e2e`, `Logs/RunLocal`) after running tests - verify events still captured
4. Start application: `./build.sh RunLocalPublish` - should start without errors
5. Test login flow manually - should work (TenantId is nullable, so users can exist without Organization for now)
