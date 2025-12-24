## Current System Analysis

This analysis documents the parts of the system relevant to the Finbuckle.MultiTenant migration goal.

### Analysis Checklist

Complete each section to ensure comprehensive understanding:

- [x] **Architecture & Layers** - Document the layers affected by the migration
- [x] **Dependencies** - Identify all dependencies that will be impacted
- [x] **Database Schema** - Document current schema and changes needed
- [x] **API Contracts** - Document current API contracts and compatibility requirements
- [x] **Cross-Cutting Concerns** - Identify logging, auditing, security, validation patterns
- [x] **Test Infrastructure** - Document current testing approach and coverage
- [x] **Build & Deployment** - Identify build and deployment considerations

### Architecture Overview

**Affected Layers:**
- **Server.Web** (Presentation): FastEndpoints for API endpoints, authentication/authorization middleware, Program.cs startup configuration
- **Server.UseCases** (Application): 19 MediatR handlers that orchestrate business logic
- **Server.Core** (Domain): Domain entities (Article, ApplicationUser, Tag, Comment, UserFollowing) and specifications
- **Server.Infrastructure** (Data Access): AppDbContext, EF Core configurations, repositories, Audit.NET integration
- **Server.FunctionalTests**: Integration tests using WebApplicationFactory and FastEndpoints.Testing
- **Server.UnitTests**: Unit tests for handlers, services, and domain logic
- **Test/e2e**: 51 Playwright E2E tests written in C#
- **Test/Postman**: 5 Newman/Postman collections for API testing

**Current Patterns:**
- **Clean Architecture**: Ardalis Clean Architecture template with Core, UseCases, Infrastructure, Web layers
- **CQRS with MediatR**: Commands and queries separated, handlers in Server.UseCases
- **Repository Pattern**: IRepository<T> and IReadRepository<T> from Ardalis.Specification
- **Specification Pattern**: Query specifications in Core for complex queries
- **ASP.NET Identity**: ApplicationUser extends IdentityUser<Guid>, stored in AspNetUsers table
- **Authentication**: Cookie-based (IdentityConstants.ApplicationScheme) for SPA, Bearer token (IdentityConstants.BearerScheme) for API
- **Domain Events**: HasDomainEventsBase with IDomainEventDispatcher in SaveChangesAsync
- **Audit Logging**: Audit.NET with AuditIdentityDbContext for EntityFramework auditing

### Critical Dependencies

**Direct Dependencies:**
| Dependency | Version | Purpose | Migration Impact |
|------------|---------|---------|------------------|
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 9.0.7 | ASP.NET Identity with EF Core | **HIGH** - Must migrate from AuditIdentityDbContext to MultiTenantIdentityDbContext |
| Audit.EntityFramework.Identity.Core | 31.3.1 | Entity Framework auditing with Identity | **HIGH** - Compatibility with Finbuckle unknown; must verify |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.7 | EF Core provider for SQL Server | **MEDIUM** - Query filters will impact all queries |
| FastEndpoints | 7.0.1 | Minimal API framework | **LOW** - Endpoints need UserContext updates for tenant claims |
| MediatR | 12.5.0 | CQRS pattern implementation | **LOW** - Handlers need tenant-aware queries |
| Ardalis.Specification | 9.2.0 | Specification pattern | **MEDIUM** - All specs querying tenant-scoped entities need updates |

**Dependency Conflicts:**
- **Finbuckle + Audit.NET**: Finbuckle.MultiTenant v10+ uses MultiTenantIdentityDbContext. Audit.NET has AuditIdentityDbContext. Need to verify compatibility or create custom DbContext that inherits from both lineages.
- **DbContext Inheritance Chain**: Current: AppDbContext → AuditIdentityDbContext → IdentityDbContext. Target: AppDbContext → MultiTenantIdentityDbContext (which also derives from IdentityDbContext). May need custom intermediate class.

### Database & Persistence

**Current Schema:**
- **AspNetUsers**: Identity users (Id, UserName, Email, PasswordHash, Bio, Image, etc.)
- **AspNetRoles**: Identity roles (Id, Name, NormalizedName)
- **AspNetUserRoles**: Many-to-many for user-role assignments
- **AspNetUserClaims**: User claims storage
- **AspNetUserLogins**: External login providers
- **AspNetUserTokens**: User tokens for Identity
- **AspNetRoleClaims**: Role claims storage
- **Articles**: Id, Title, Description, Body, Slug, AuthorId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, ChangeCheck
- **Tags**: Id, Name, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, ChangeCheck
- **Comments**: Id, Body, ArticleId, AuthorId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, ChangeCheck
- **UserFollowings**: FollowerId, FollowedId (composite key for many-to-many self-referencing)
- **ArticleTags**: Many-to-many join table (ArticlesId, TagsId)
- **ArticleFavorites**: Many-to-many join table (ArticleId, FavoritedById)

**Schema Changes Required:**
- **Add Organization table**: Id (Guid), Name, Identifier (for Finbuckle), CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, ChangeCheck
- **Add TenantId to AspNetUsers**: Foreign key to Organizations table
- **Add TenantId to Articles**: Foreign key for tenant isolation
- **Add TenantId to Tags**: Foreign key for tenant isolation (tags are tenant-scoped)
- **Add TenantId to Comments**: Foreign key for tenant isolation
- **Create indexes on TenantId columns**: For query performance with filters
- **UserFollowings remains unchanged**: Following relationships are within same organization (no cross-org following)
- **Add TenantInfo store table** (if using EF Core store): For Finbuckle tenant configuration

**Data Migration Strategy:**
- **Preserving existing data**: No - per requirements, this is pre-production with no users
- **Migration complexity**: High - requires structural changes to multiple tables and relationships
- **Rollback strategy**: Use EF Core migrations; each phase should be reversible via `dotnet ef migrations remove`

### API Contracts & Clients

**Current Endpoints:**
| Endpoint | Method | Auth | Clients | Breaking Change? |
|----------|--------|------|---------|------------------|
| POST /api/identity/login | POST | Anonymous | React SPA | No - login flow extended |
| POST /api/identity/register | POST | Anonymous | React SPA | Yes - Creates Organization |
| GET /api/user | GET | Required | React SPA | No - claims enhanced |
| PUT /api/user | PUT | Required | React SPA | No |
| GET /api/profiles/:username | GET | Optional | React SPA | No |
| POST /api/profiles/:username/follow | POST | Required | React SPA | No |
| DELETE /api/profiles/:username/follow | DELETE | Required | React SPA | No |
| GET /api/articles | GET | Optional | React SPA | No - filtered by tenant |
| GET /api/articles/feed | GET | Required | React SPA | No - filtered by tenant |
| GET /api/articles/:slug | GET | Optional | React SPA | No - filtered by tenant |
| POST /api/articles | POST | Required | React SPA | No |
| PUT /api/articles/:slug | PUT | Required | React SPA | No |
| DELETE /api/articles/:slug | DELETE | Required | React SPA | No |
| POST /api/articles/:slug/comments | POST | Required | React SPA | No |
| GET /api/articles/:slug/comments | GET | Optional | React SPA | No - filtered by tenant |
| DELETE /api/articles/:slug/comments/:id | DELETE | Required | React SPA | No |
| POST /api/articles/:slug/favorite | POST | Required | React SPA | No |
| DELETE /api/articles/:slug/favorite | DELETE | Required | React SPA | No |
| GET /api/tags | GET | Anonymous | React SPA | No - filtered by tenant |

**Breaking Changes:**
- **Registration (POST /api/identity/register)**: Will now create an Organization and assign user as Owner. Response unchanged, but semantics different.
- **Data Visibility**: All list endpoints will be automatically filtered by TenantId via global query filters. No API contract changes, but behavior changes significantly.
- **New Admin Endpoint (Future)**: Will need new endpoint for organization owners to invite/manage users (out of scope for initial phases).

### Cross-Cutting Concerns

**Logging:**
- **Current approach**: Serilog configured in Program.cs, logs to console and file
- **Migration impact**: Need to include TenantId in log context for all operations. Use Serilog enrichers to add tenant context.
- **Location**: `App/Server/src/Server.Web/Configurations/LoggerConfig.cs` (if exists) or in Program.cs

**Auditing:**
- **Current approach**: Audit.NET with AuditIdentityDbContext, captures EntityFramework changes
- **Migration impact**: HIGH - Must ensure TenantId is captured in audit logs. May need custom audit configuration.
- **Compatibility issues**: AuditIdentityDbContext vs MultiTenantIdentityDbContext - need to verify if both can coexist or if custom implementation needed
- **Location**: `App/Server/src/Server.Infrastructure/Data/AppDbContext.cs` and `AuditConfiguration.cs`

**Security:**
- **Current authentication**: ASP.NET Identity with UserManager, SignInManager
- **Current authorization**: Policy-based authorization (if used), AuthSchemes on FastEndpoints
- **Migration changes**: 
  - Add `TenantId` claim during sign-in via IClaimsTransformation
  - Finbuckle ClaimStrategy reads TenantId claim to resolve tenant
  - Middleware ordering critical: UseMultiTenant() before UseAuthentication()
  - Authorization policies may need tenant-aware checks

**Validation:**
- **Current approach**: FluentValidation validators in Server.Web for endpoint DTOs
- **Migration impact**: LOW - Validation logic mostly unchanged, but some validators may need tenant-aware checks (e.g., unique slug within organization)
- **Location**: `App/Server/src/Server.Web/**/Validators/`

### Test Infrastructure

**Current Test Types:**
- **Unit tests**: ~15 tests in Server.UnitTests covering handlers, services, domain logic
  - Pattern: xUnit with NSubstitute for mocking
  - Coverage: Infrastructure services (UserContext, UnitOfWork), domain logic, MediatR behaviors
- **Functional tests**: ~45 tests in Server.FunctionalTests
  - Pattern: xUnit with WebApplicationFactory, FastEndpoints.Testing extensions
  - Coverage: End-to-end API testing with real database (in-memory or test container)
  - Uses fixtures (ArticlesFixture, ProfilesFixture) for test setup with pre-created users
- **E2E tests**: 51 Playwright tests in Test/e2e/E2eTests
  - Pattern: xUnit with Microsoft.Playwright for browser automation
  - Coverage: Full user flows through React SPA
- **API tests**: 5 Postman/Newman collections in Test/Postman
  - Coverage: Comprehensive API contract testing
  - Collections: Auth, Articles, ArticlesEmpty, FeedAndArticles, Profiles

**Test Maintenance During Migration:**
- **Functional tests requiring updates**: ~45 tests - All fixtures need tenant context setup (use Finbuckle's StaticStrategy for tests)
- **E2E tests requiring updates**: ~51 tests - Database wipe scripts need to handle Organizations table
- **Postman tests requiring updates**: All 5 collections - Registration flow creates Organization; queries filtered by tenant
- **New tests required**: 
  - Multi-tenancy isolation tests (verify users can't see other org's data)
  - Organization creation tests
  - Claims transformation tests
  - Tenant resolution tests

### Handler/Service Dependencies

**Critical Handler Dependencies:**
| Handler/Service | Dependencies | Usage Count | Migration Complexity |
|-----------------|--------------|------------|---------------------|
| CreateArticleHandler | UserManager, IRepository<Article>, IRepository<Tag> | 1 endpoint | **MEDIUM** - Needs TenantId from tenant context |
| ListArticlesHandler | IReadRepository<Article> | 1 endpoint | **LOW** - Query filters auto-apply |
| GetFeedHandler | UserManager, IReadRepository<Article> | 1 endpoint | **LOW** - Query filters auto-apply |
| GetArticleHandler | IReadRepository<Article> | 1 endpoint | **LOW** - Query filters auto-apply |
| UpdateArticleHandler | IRepository<Article> | 1 endpoint | **LOW** - Query filters auto-apply |
| DeleteArticleHandler | IRepository<Article> | 1 endpoint | **MEDIUM** - Verify ownership within org |
| FavoriteArticleHandler | UserManager, IRepository<Article> | 1 endpoint | **LOW** - Query filters auto-apply |
| UnfavoriteArticleHandler | UserManager, IRepository<Article> | 1 endpoint | **LOW** - Query filters auto-apply |
| CreateCommentHandler | UserManager, IRepository<Article> | 1 endpoint | **MEDIUM** - Needs TenantId from tenant context |
| GetCommentsHandler | IReadRepository<Article> | 1 endpoint | **LOW** - Query filters auto-apply |
| DeleteCommentHandler | IRepository<Article> | 1 endpoint | **MEDIUM** - Verify ownership within org |
| GetProfileHandler | UserManager | 1 endpoint | **LOW** - Users auto-filtered by tenant |
| FollowUserHandler | UserManager, IRepository<ApplicationUser> | 1 endpoint | **LOW** - Following within org only |
| UnfollowUserHandler | UserManager, IRepository<ApplicationUser> | 1 endpoint | **LOW** - Following within org only |
| GetCurrentUserHandler | UserManager | 1 endpoint | **LOW** - User already tenant-scoped |
| UpdateUserHandler | UserManager | 1 endpoint | **LOW** - User already tenant-scoped |
| ListTagsHandler | IReadRepository<Tag> | 1 endpoint | **LOW** - Query filters auto-apply |

**Ripple Effects:**
- **Changing AppDbContext to MultiTenantIdentityDbContext** requires:
  - Update all handlers using IRepository to work with tenant-aware queries
  - Update all specifications to be tenant-aware (some may need explicit tenant checks)
  - Update test fixtures to configure tenant context
  - Update functional tests to use StaticStrategy for tenant resolution
  - Update E2E tests to handle Organization table in database wipe scripts
  - Update Postman tests for changed registration semantics
- **Adding TenantId to ApplicationUser** requires:
  - Update ApplicationUserConfiguration in EF Core
  - Create migration for schema change
  - Update registration flow (Identity endpoints or custom)
  - Update IClaimsTransformation to add TenantId claim
  - Update all queries/handlers that load users (already using UserManager, should auto-filter)
- **Adding TenantId to domain entities (Article, Tag, Comment)** requires:
  - Update entity classes with TenantId property
  - Update entity configurations with IsMultiTenant() or [MultiTenant] attribute
  - Create migrations for schema changes
  - Update handlers that create entities to set TenantId from tenant context
  - Update specifications (may not be needed if global query filters work)

**Shared Utilities:**
- **IUserContext** (Server.Infrastructure/Services/UserContext.cs): Gets current user ID from HttpContext. Will need to also provide TenantId.
- **UserManager<ApplicationUser>**: Used by almost all handlers. Will auto-filter by tenant once Identity is configured.
- **IRepository<T> / IReadRepository<T>**: Used by all handlers. Will auto-apply query filters once entities are marked as multi-tenant.
- **Specifications**: 17 specification classes in Server.Core - most will auto-work with query filters, but some may need explicit tenant checks.

### Key Observations for Migration

1. **Identity Integration is Critical Path**: The migration to MultiTenantIdentityDbContext and integration with Audit.NET is the highest-risk item. This affects authentication, all handlers, and the entire data access layer. This must be researched thoroughly and potentially prototyped before proceeding.

2. **Test Maintenance is Significant**: With 45 functional tests, 51 E2E tests, and 5 Postman collections, test maintenance will be a major effort in every phase. Each phase must explicitly account for test updates or risk breaking CI/CD.

3. **Global Query Filters Are Double-Edged**: Finbuckle's automatic query filtering is powerful but can hide bugs. Must be very careful with:
   - Navigation properties (Include statements may filter unexpectedly)
   - Specifications (some may need explicit tenant checks)
   - Audit logging (must ensure tenant context captured)
   - Testing (need both tenant-isolated tests and cross-tenant isolation tests)

4. **Registration Flow is Semantically Different**: Currently, registration creates a user. Post-migration, registration creates an Organization AND a user (as Owner). This is a significant behavioral change that impacts tests and potentially frontend flows.

5. **Middleware Ordering is Critical**: UseMultiTenant() must come BEFORE UseAuthentication() for ClaimStrategy to work. This is non-obvious and easy to get wrong.

6. **No Cross-Tenant References**: The design enforces that users belong to exactly one organization and can only see/manipulate data in their organization. This simplifies the migration (no multi-tenancy at user level) but means we can ignore cross-tenant scenarios.

7. **Audit.NET + Finbuckle Compatibility Unknown**: The combination of AuditIdentityDbContext and MultiTenantIdentityDbContext is not documented in references. This is a research task that must be completed in the key-decisions phase before detailed planning.

8. **Slug Uniqueness Changes**: Currently, article slugs are globally unique. Post-migration, slugs only need to be unique within an organization. This may require updating validation logic and database constraints.

9. **Organization Management Out of Scope**: Requirements explicitly exclude admin screens for user management. This simplifies Phase 1 but means we defer complexity to future phases.

10. **Clean Slate for Data**: Requirements state this is pre-production with no users, so no data migration needed. This is a massive simplification that allows us to make breaking schema changes freely.