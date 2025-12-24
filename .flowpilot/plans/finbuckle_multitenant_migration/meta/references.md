# References for Finbuckle.Multitenant Migration

## Research Checklist

Before creating the migration plan, ensure comprehensive research in these areas:

- [x] Official Microsoft/vendor documentation searched and reviewed
- [x] Community best practices and common pitfalls identified
- [x] Breaking changes and migration guides reviewed
- [x] Database/persistence layer implications researched
- [x] Testing strategies for the migration researched
- [x] Performance implications documented
- [x] Security implications documented
- [x] Compatibility with existing dependencies verified
- [x] Alternative approaches evaluated and compared

## Official Documentation

### [Finbuckle.MultiTenant - Identity Integration](https://github.com/Finbuckle/Finbuckle.MultiTenant/blob/main/docs/Identity.md)
**Key Takeaways:**
- MultiTenant has built-in support for ASP.NET Core Identity data isolation when using EF Core
- Derive from `MultiTenantIdentityDbContext` (which itself derives from `IdentityDbContext`) for automatic multi-tenant configuration
- In v10+, all Identity entity types are configured as multi-tenant by default when using MultiTenantIdentityDbContext variants
- Unique indexes automatically include TenantId property
- Identity schema version 3 supports passkeys (WebAuthn) with multi-tenancy
- Can override behavior with `IsNotMultiTenant()` extension for specific entities

**Relevance:** This is the primary approach for integrating Identity with multi-tenancy. We'll derive from MultiTenantIdentityDbContext to get automatic tenant isolation for all Identity entities.

### [Finbuckle.MultiTenant - EF Core Integration](https://github.com/Finbuckle/Finbuckle.MultiTenant/blob/main/docs/EFCore.md)
**Key Takeaways:**
- Two main approaches: derive from `MultiTenantDbContext` or implement `IMultiTenantDbContext` interface
- Use `[MultiTenant]` attribute or `.IsMultiTenant()` fluent API to mark entities as tenant-scoped
- Global query filters automatically applied to restrict queries by TenantId
- Shadow properties used by default for TenantId
- Added entities automatically associated with current tenant; mismatch throws `MultiTenantException`
- Can use `.IsNotMultiTenant()` to exclude specific entities
- Design-time instantiation requires a design-time factory with dummy TenantInfo
- Factory instantiation pattern recommended for creating contexts with tenant info

**Relevance:** We'll use the fluent API approach to mark domain entities as multi-tenant and leverage automatic query filtering for data isolation.

### [Finbuckle.MultiTenant - Core Concepts](https://github.com/Finbuckle/Finbuckle.MultiTenant/blob/main/docs/CoreConcepts.md)
**Key Takeaways:**
- `TenantInfo` is a record with Id, Identifier, and Name properties
- Should create custom TenantInfo class with additional properties as needed
- `MultiTenantContext<TTenantInfo>` contains current tenant information
- Strategies determine tenant identifier; Stores retrieve TenantInfo from identifier
- Multiple strategies can be chained; first non-null result wins
- Can obtain current tenant via `HttpContext.GetMultiTenantContext()`

**Relevance:** We'll create a custom TenantInfo that maps to our Organization entity with necessary properties.

### [Finbuckle.MultiTenant - Configuration and Usage](https://github.com/Finbuckle/Finbuckle.MultiTenant/blob/main/docs/ConfigurationAndUsage.md)
**Key Takeaways:**
- Configure via `AddMultiTenant<TTenantInfo>()` in DI
- Chain strategies and stores: `.WithClaimStrategy()`, `.WithEFCoreStore()`, etc.
- Use `UseMultiTenant()` middleware in ASP.NET Core pipeline
- Middleware must run BEFORE authentication for ClaimStrategy to work properly
- Can exclude specific endpoints from tenant resolution
- Short-circuit options when tenant not resolved

**Relevance:** Critical middleware ordering: UseMultiTenant() before UseAuthentication(). ClaimStrategy will read TenantId claim from authenticated user.

### [ASP.NET Core - Claims Transformation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/claims?view=aspnetcore-10.0)
**Key Takeaways:**
- `IClaimsTransformation` interface allows adding/modifying claims after authentication
- `TransformAsync` may be called multiple times; check if claim already exists
- Register as transient or scoped service
- Can pull additional data from database to enrich claims
- Used to add custom claims like tenant identifiers to user principal

**Relevance:** We'll implement IClaimsTransformation to add TenantId claim to user principal after sign-in, enabling ClaimStrategy to resolve tenant.

### [Entity Framework Core - Global Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
**Key Takeaways:**
- Query filters act like automatic WHERE clause on every query
- Use `HasQueryFilter()` in OnModelCreating
- EF 10+ supports named query filters for multiple filters per entity
- Can disable filters with `IgnoreQueryFilters()` for specific queries
- Be careful with required navigations; may need optional to avoid unexpected filtering
- Query filters reference context state (like tenant ID)
- Only defined for root entity types in inheritance hierarchy

**Relevance:** Finbuckle leverages this feature for automatic tenant isolation. Important to understand limitations with navigations and inheritance.

### [Entity Framework Core - Multi-tenancy](https://learn.microsoft.com/en-us/ef/core/miscellaneous/multitenancy)
**Key Takeaways:**
- Three main approaches: separate databases, shared database with discriminator column, or hybrid
- Shared database approach uses query filters on discriminator column
- Must be careful with migrations in per-tenant database scenarios
- DbContext pooling requires special handling for tenant state
- Consider performance implications of discriminator columns on large tables

**Relevance:** We're using shared database with discriminator column (TenantId). Need to handle DbContext pooling carefully or avoid it.

## Community Resources & Best Practices

### [Multi-tenancy with ASP.NET Core and Finbuckle.Multitenant](https://khalidabuhakmeh.com/multi-tenancy-with-aspnet-core-and-finbuckle-multitenant)
**Key Takeaways:**
- Comprehensive tutorial on setting up Finbuckle with ASP.NET Core
- Demonstrates multiple resolution strategies
- Shows how to set up EF Core store for tenant configuration
- Practical examples of tenant-aware queries

**Relevance:** Good reference for end-to-end setup patterns and common configuration scenarios.

### [ClaimStrategy Best Practices Discussion (GitHub Issue #316)](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/316)
**Key Takeaways:**
- UseAuthentication must come AFTER UseMultiTenant for ClaimStrategy
- Claims won't exist until after login, so ClaimStrategy needs fallback or special handling for registration flows
- Consider combining with other strategies (header, route) for pre-authentication scenarios
- Use dedicated claim like "tenant_id" or "TenantId", avoid generic claims
- Cookies are cached efficiently, so middleware ordering doesn't create performance issues

**Relevance:** Critical for our implementation. We need to handle the registration flow where new users create new organizations before authentication completes.

### [Integration Testing with Finbuckle (GitHub Issue #830)](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/830)
**Key Takeaways:**
- Use `WithStaticStrategy("tenant-id")` for integration tests
- WebApplicationFactory requires custom configuration to inject tenant context
- Can manually craft HttpContext with required headers/claims for tests
- Mock `IMultiTenantContextAccessor<T>` for unit tests

**Relevance:** We'll need to update integration tests to handle tenant context. Static strategy for tests is the simplest approach.

## Known Issues & Pitfalls

### [Identity Data Isolation Issues](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/80)
**Description:** Known issues with Identity and multi-tenancy including authentication redirect bugs and UI compatibility
**Impact:** 
- Social login redirect handling may fail without WithRemoteAuthentication()
- Identity UI may not render correctly (Bootstrap version mismatch)
- External authentication providers may not associate users with correct tenant
**Mitigation:** 
- We're not using social login, so redirect issues don't apply
- We're not using Identity UI scaffolding, using custom React UI
- Manually ensure TenantId is set correctly during registration

### [TenantInfo Null in Tests](https://stackoverflow.com/questions/78275697/finbuckle-multitenant-tenantinfo-being-null-in-integration-test-under-webapplic)
**Description:** Integration tests fail because TenantInfo is null when using WebApplicationFactory
**Impact:** Tests that depend on tenant context will fail without proper configuration
**Mitigation:** Use StaticStrategy or manually configure tenant resolution in test setup

### [Query Filter Performance on Large Tables](https://stackoverflow.com/questions/79001955/ef-core-multitenancy-using-a-dynamic-global-filter)
**Description:** Query filters add WHERE clause to every query, which can impact performance on large tables without proper indexing
**Impact:** Poor query performance if TenantId column not indexed
**Mitigation:** 
- Create indexes on TenantId columns for all tenant-scoped entities
- Monitor query execution plans
- Consider denormalization for read-heavy scenarios

### [Required Navigation Issues with Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters#query-filters-and-required-navigations)
**Description:** Required navigations with filtered entities can cause unexpected behavior where parent entities get filtered out
**Impact:** Queries with Include() on required navigations may return fewer results than expected
**Mitigation:** 
- Make navigations optional where appropriate
- Apply consistent filters on both sides of relationships
- Test Include() queries carefully during implementation

### [Breaking Changes in v10](https://github.com/Finbuckle/Finbuckle.MultiTenant/blob/main/CHANGELOG.md)
**Description:** Version 10.0.0 introduces significant breaking changes
**Impact:**
- TenantInfo is now a record instead of class
- MultiTenantIdentityDbContext entities are multi-tenant by default
- Namespace changes for stores and options
- Named filters replace anonymous filters
- Options package split out
**Mitigation:** 
- Use latest version (v10+) from the start to avoid migration issues later
- Follow new patterns for TenantInfo customization
- Be aware of namespace changes when referencing documentation

### [Audit.NET Integration Considerations](https://github.com/Finbuckle/Finbuckle.MultiTenant)
**Description:** Audit.NET doesn't natively understand multi-tenancy
**Impact:** 
- Audit logs may not automatically capture TenantId
- Need to manually include tenant context in audit events
**Mitigation:** 
- Configure Audit.NET to include TenantId from current tenant context
- Ensure audit logs capture tenant identifier for all operations
- Test audit logging thoroughly in multi-tenant scenarios

## Alternative Approaches Considered

### Per-Tenant Databases
**Description:** Each organization gets its own physical database with separate connection string
**Pros:**
- Complete data isolation at database level
- Easier to backup/restore individual tenants
- Can use different database sizes/tiers per tenant
- No risk of cross-tenant data leakage through code bugs

**Cons:**
- More complex connection string management
- Migrations must run against all tenant databases
- More expensive (multiple databases)
- Harder to implement cross-tenant reporting
- Increased operational complexity

**Decision:** Rejected. Requirements specify single-database approach. Overkill for the internal company social network use case.

### Custom Multi-Tenancy Without Library
**Description:** Implement multi-tenancy manually without Finbuckle
**Pros:**
- Complete control over implementation
- No external dependency
- Can customize exactly to needs

**Cons:**
- Significant development effort
- Easy to make security mistakes
- Must implement strategy pattern, stores, query filters manually
- Reinventing well-tested wheel

**Decision:** Rejected. Finbuckle is mature, well-maintained, and handles complex edge cases we'd otherwise have to discover through bugs.

### ASP.NET Core Identity Stores per Tenant
**Description:** Use separate Identity stores per tenant instead of shared tables with filters
**Pros:**
- Complete Identity data isolation
- Simpler Identity configuration

**Cons:**
- Requires custom UserStore and RoleStore implementation
- More complex than using MultiTenantIdentityDbContext
- Doesn't align with single-database requirement

**Decision:** Rejected. MultiTenantIdentityDbContext provides simpler integration with adequate isolation.

### Route-Based Tenant Resolution
**Description:** Use route parameter (e.g., /api/{tenant}/articles) instead of claims
**Pros:**
- Tenant visible in URL
- Easy to test with tools like Postman
- Works before authentication

**Cons:**
- Must include tenant in every route
- More complex routing configuration
- Can't leverage existing authenticated user's organization
- User could potentially access other tenants by manipulating URL

**Decision:** Rejected. ClaimStrategy is more secure and simpler for authenticated users. The requirement states users belong to exactly one organization, so claim-based resolution is natural.

### Header-Based Tenant Resolution
**Description:** Use X-Tenant-Id header for tenant resolution
**Pros:**
- Clean URLs
- Works for both authenticated and unauthenticated requests
- Common in API-first applications

**Cons:**
- Client must manage and send header
- Security concerns if not validated against authenticated user
- Extra work for frontend to include header in every request

**Decision:** Rejected. ClaimStrategy is more secure and requires no client-side header management. The authenticated user's claim naturally carries the tenant identifier.