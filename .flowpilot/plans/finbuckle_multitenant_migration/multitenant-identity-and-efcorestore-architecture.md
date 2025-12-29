# How MultiTenantIdentityDbContext Works with EFCoreStoreDbContext

## Summary

**They are SEPARATE DbContexts with DIFFERENT purposes:**

- **`EFCoreStoreDbContext<TTenantInfo>`**: Stores **tenant metadata** (tenant directory/catalog)
- **`MultiTenantIdentityDbContext`**: Stores **per-tenant Identity data** (users, roles, etc.)

These two contexts are **NOT meant to be combined** - they serve completely different roles in the Finbuckle architecture.

---

## Architecture Overview

### Two Separate Concerns

#### 1. Tenant Resolution/Storage (EFCoreStoreDbContext)

**Purpose**: Store the list of tenants and their metadata

**What it stores**:
- Tenant identifiers
- Tenant names
- Connection strings (if per-tenant databases)
- Custom tenant properties

**DbContext**: Derives from `EFCoreStoreDbContext<TTenantInfo>`

**Table**: `TenantInfo` (or custom name)

**Usage**: Used by Finbuckle's `IMultiTenantStore<TTenantInfo>` during tenant resolution

#### 2. Per-Tenant Application Data (MultiTenantIdentityDbContext)

**Purpose**: Store tenant-isolated application data including Identity

**What it stores**:
- Users (per tenant)
- Roles (per tenant)
- Claims (per tenant)
- All other application entities (per tenant)

**DbContext**: Derives from `MultiTenantIdentityDbContext` or `MultiTenantIdentityDbContext<TUser>`

**Tables**: `AspNetUsers`, `AspNetRoles`, etc. - all with `TenantId` column

**Usage**: Used by application code and ASP.NET Core Identity for CRUD operations

---

## Typical Setup Pattern

### Step 1: Create Tenant Store Context (for tenant metadata)

```csharp
// Stores ONLY TenantInfo - the directory of tenants
public class TenantStoreDbContext : EFCoreStoreDbContext<TenantInfo>
{
    public TenantStoreDbContext(DbContextOptions<TenantStoreDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Configures TenantInfo table
        modelBuilder.HasDefaultSchema("dbo");
    }
}
```

### Step 2: Create Application/Identity Context (for per-tenant data)

```csharp
// Stores per-tenant application data including Identity
public class AppDbContext : MultiTenantIdentityDbContext<ApplicationUser>
{
    public AppDbContext(
        IMultiTenantContextAccessor<TenantInfo> multiTenantContextAccessor,
        DbContextOptions<AppDbContext> options)
        : base(multiTenantContextAccessor, options)
    {
    }

    // Your application entities
    public DbSet<Article> Articles { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Configures Identity tables with TenantId

        // Configure your entities as multi-tenant
        modelBuilder.Entity<Article>().IsMultiTenant();
        modelBuilder.Entity<Comment>().IsMultiTenant();
    }
}
```

### Step 3: Register Services

```csharp
// Configure MultiTenant with EFCore tenant store
services.AddMultiTenant<TenantInfo>()
    .WithClaimStrategy("TenantId")
    .WithEFCoreStore<TenantStoreDbContext, TenantInfo>();

// Register TenantStoreDbContext (separate from app context)
services.AddDbContext<TenantStoreDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("TenantStore"),
        b => b.MigrationsHistoryTable("__EFMigrationsHistory_TenantStore")));

// Register app/identity context
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("AppDatabase")));

// Configure Identity to use AppDbContext
services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
```

---

## Key Architectural Principles

### 1. Separate Schemas

| Context | Purpose | Tables |
|---------|---------|--------|
| `TenantStoreDbContext` | Tenant directory | `TenantInfo` only |
| `AppDbContext` | Application data | `AspNetUsers`, `AspNetRoles`, `Articles`, `Comments`, etc. (all with `TenantId`) |

### 2. No Table Overlap

**Critical**: These contexts MUST have **zero table overlap**

- `TenantInfo` table is ONLY in `TenantStoreDbContext`
- Identity tables are ONLY in `AppDbContext`
- Application tables are ONLY in `AppDbContext`

### 3. No Cross-Context Foreign Keys

Since they're separate DbContexts, you **cannot create foreign keys** between them:

- `AspNetUsers.TenantId` is a **plain string** column (no FK)
- The relationship to `TenantInfo` is **logical only**, enforced by application code
- This is by design - EF Core doesn't support FKs across DbContexts

### 4. Separate Migrations

Each context maintains its own migrations:

```bash
# TenantStore migrations
dotnet ef migrations add InitialTenantStore \
    --context TenantStoreDbContext \
    --output-dir Data/Migrations/TenantStore

# AppDbContext migrations
dotnet ef migrations add AddIdentity \
    --context AppDbContext \
    --output-dir Data/Migrations
```

### 5. Migration Execution Order

On application startup:

```csharp
// Apply TenantStore migrations FIRST
var tenantStoreContext = scope.ServiceProvider.GetRequiredService<TenantStoreDbContext>();
await tenantStoreContext.Database.MigrateAsync();

// Then apply AppDbContext migrations
var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await appContext.Database.MigrateAsync();
```

---

## How Tenant Resolution Works

### During HTTP Request

1. **Tenant Resolution**: Middleware resolves tenant using strategy (e.g., claims, host, route)
2. **Tenant Lookup**: `IMultiTenantStore<TenantInfo>` queries `TenantStoreDbContext` to get tenant metadata
3. **Tenant Context Set**: Resolved `TenantInfo` is stored in `IMultiTenantContextAccessor`
4. **App Data Access**: `AppDbContext` uses `IMultiTenantContextAccessor` to apply query filters
5. **Isolated Data**: All queries automatically filtered by `TenantId`

### Visual Flow

```
HTTP Request
    ↓
Tenant Resolution Middleware
    ↓
IMultiTenantStore<TenantInfo> (uses TenantStoreDbContext)
    ↓
Resolve TenantInfo from TenantInfo table
    ↓
Store in IMultiTenantContextAccessor
    ↓
AppDbContext (MultiTenantIdentityDbContext)
    ↓
Apply query filters using TenantId from accessor
    ↓
Return only current tenant's data
```

---

## Common Misunderstandings

### ❌ Misconception: "AppDbContext should inherit from both EFCoreStoreDbContext AND MultiTenantIdentityDbContext"

**Reality**: C# doesn't allow multiple inheritance. These are **separate contexts** for **separate purposes**.

### ❌ Misconception: "TenantInfo should be in AppDbContext"

**Reality**: `TenantInfo` ONLY belongs in `TenantStoreDbContext`. Putting it in both causes migration conflicts (the exact issue we encountered in Phase 7).

### ❌ Misconception: "I need a foreign key from AspNetUsers.TenantId to TenantInfo.Id"

**Reality**: Foreign keys cannot span DbContexts. The relationship is logical only, enforced by application code.

### ❌ Misconception: "I can use a single DbContext for everything"

**Reality**: Finbuckle's `WithEFCoreStore<TDbContext, TTenantInfo>` **requires** `TDbContext : EFCoreStoreDbContext<TTenantInfo>`. This is enforced by generic type constraints across all versions (v6.x, v7.x, v8.x, v9.x, v10.x).

---

## Why Separate Contexts?

### Architectural Benefits

1. **Separation of Concerns**
   - Tenant metadata vs. tenant data
   - Different lifecycles and access patterns

2. **Migration Safety**
   - Changes to tenant storage don't affect application schema
   - Independent versioning

3. **Flexibility**
   - Can use different databases
   - Can use different migration strategies
   - Easier to scale independently

4. **Finbuckle's Design**
   - `EFCoreStore` expects dedicated context
   - Follows single responsibility principle
   - Aligns with official samples and documentation

### Real-World Benefits We Discovered

During Phase 7, we discovered why this separation is critical:

- **Migration Conflict**: When we tried to have `TenantInfo` in `AppDbContext`, migrations failed because:
  1. `TenantStoreDbContext` creates `TenantInfo` table (via Finbuckle)
  2. `AppDbContext` migration tried to create same table
  3. Migration failed with "object already exists"
  4. Failed migration not recorded in `__EFMigrationsHistory`
  5. Subsequent fix migrations never ran

This failure **proved** the architectural requirement for separation.

---

## Official Documentation References

### Finbuckle Documentation

From https://www.finbuckle.com/MultiTenant/Docs/EFCore:

> The EFCoreStore is just for listing out tenants. You can use other store types if preferable; EFCoreStore isn't mandatory for tenant resolution.

From https://github.com/Finbuckle/Finbuckle.MultiTenant/discussions/664:

> EFCoreStoreDbContext: Used only for storing/resolving tenant metadata, NOT for app/identity data.
> 
> MultiTenantIdentityDbContext: Used for tenant-aware ASP.NET Core Identity data (users, roles, claims).
> 
> No Multiple Inheritance: C# does not allow DbContext to inherit from both EFCoreStoreDbContext and MultiTenantIdentityDbContext. Keep them as separate contexts, each configured appropriately.

### Official Sample Projects

Finbuckle's official samples follow this pattern:
- https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples/IdentityAppSample

---

## Our Implementation (Correct)

### Current Architecture (Phase 7)

✅ **TenantStoreDbContext**
- Derives from `EFCoreStoreDbContext<TenantInfo>`
- Manages ONLY `TenantInfo` table
- Separate migrations in `Data/Migrations/TenantStore/`
- Separate history table: `__EFMigrationsHistory_TenantStore`

✅ **AppDbContext**  
- Derives from `MultiTenantIdentityDbContext<ApplicationUser>`
- Manages Identity tables + application entities
- Separate migrations in `Data/Migrations/`
- Standard history table: `__EFMigrationsHistory`

✅ **No Table Overlap**
- `TenantInfo` ONLY in `TenantStoreDbContext`
- All other tables ONLY in `AppDbContext`

✅ **No Cross-Context FKs**
- `AspNetUsers.TenantId` is plain string (no FK)
- Relationship enforced by application logic

✅ **Proper Registration**
- `AddMultiTenant<TenantInfo>().WithEFCoreStore<TenantStoreDbContext, TenantInfo>()`
- Separate `AddDbContext` for each context

---

## Conclusion

The current Phase 7 implementation follows Finbuckle's required architecture:

1. ✅ **Separate DbContexts**: `TenantStoreDbContext` (tenant metadata) vs `AppDbContext` (application/identity data)
2. ✅ **Zero Table Overlap**: Each context manages completely separate tables
3. ✅ **No Cross-Context FKs**: Relationships are logical only
4. ✅ **Independent Migrations**: Each context has its own migration history
5. ✅ **Proper Service Registration**: Uses Finbuckle's standard APIs

The only remaining issue is cleaning up the broken migrations that tried to mix these concerns. Once that's done, the architecture will be correct and aligned with Finbuckle's design.

---

## Version Verification

This architecture is **consistent across all Finbuckle versions**:

- ✅ v6.13.1 (current project version)
- ✅ v7.0.2 (previously used in project)
- ✅ v8.0.5 
- ✅ v9.x
- ✅ v10.0.1 (latest)

The requirement for `TDbContext : EFCoreStoreDbContext<TTenantInfo>` is enforced by generic type constraints in the source code and has never changed across major versions.
