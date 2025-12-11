## Decision 1: Audit.NET and Identity DbContext Integration

The application currently uses `Audit.EntityFramework.AuditDbContext` as the base class for `AppDbContext`, while ASP.NET Core Identity requires `IdentityDbContext` as the base class. We need to choose an integration approach.

### Option A: Use AuditIdentityDbContext

**Description:** Use Audit.NET's `AuditIdentityDbContext` from the `Audit.EntityFramework.Identity` package, which combines both Audit.NET and Identity capabilities in a single base class.

**Pros:**
- Audit.NET officially supports this pattern via dedicated package
- Single inheritance chain: AppDbContext → AuditIdentityDbContext → IdentityDbContext
- Maintains full auditing capabilities for Identity operations
- Can track user management actions, role assignments, password changes, sign-in attempts
- Simplest integration approach

**Cons:**
- Introduces dependency on additional NuGet package (`Audit.EntityFramework.Identity`)
- Less flexibility if we need to customize auditing behavior differently for Identity vs. application entities
- May include auditing overhead for Identity operations we don't need

**Impact:** Minimal code changes required. Straightforward migration path with official support. Maintains consistency with current auditing approach.

### Option B: Use SaveChangesInterceptor Pattern

**Description:** Keep Identity's `IdentityDbContext` as base class and use `AuditSaveChangesInterceptor` for auditing application entities, treating Identity entities separately.

**Pros:**
- More modern EF Core pattern (interceptors over inheritance)
- Flexible - can selectively audit application entities without auditing Identity operations
- Easier to test and mock
- Follows composition over inheritance principle
- Better separation of concerns

**Cons:**
- Requires more significant refactoring of current codebase
- Identity operations (user registration, password changes) won't be automatically audited
- Need to manually configure interceptor and scope
- More complex setup compared to simple inheritance

**Impact:** Moderate code changes required. Need to refactor from AuditDbContext inheritance to interceptor pattern. May need to add custom auditing for critical Identity operations.

### Option C: Dual DbContext Pattern

**Description:** Maintain separate DbContexts - one for application entities with Audit.NET, and one for Identity entities.

**Pros:**
- Complete separation of concerns
- Each DbContext optimized for its purpose
- No inheritance conflicts
- Can use different database connections if needed

**Cons:**
- Cannot use transactions across contexts easily
- Significant architectural change
- Doubles database connection overhead
- Complex coordination between contexts
- Not aligned with clean architecture single DbContext principle

**Impact:** Major architectural changes. Requires careful transaction management. Significant complexity increase.

### Choice

- [x] Option A: Use AuditIdentityDbContext
- [ ] Option B: Use SaveChangesInterceptor Pattern  
- [ ] Option C: Dual DbContext Pattern

**Rationale:** Option A provides the simplest migration path with official Audit.NET support through the `Audit.EntityFramework.Identity` package. It maintains our current auditing approach while adding Identity capabilities. The package is designed specifically for this use case and is actively maintained. The added dependency is minimal and the pattern is well-documented. Option B would be more modern but requires significant refactoring that's out of scope for this migration. Option C introduces unnecessary complexity.

---

## Decision 2: User Entity Customization Strategy

ASP.NET Core Identity uses `IdentityUser` as its base user class. Our current `User` entity has custom properties (Bio, Image) and relationships (Following/Followers) that need to be preserved.

### Option A: Extend IdentityUser with ApplicationUser

**Description:** Create `ApplicationUser : IdentityUser<Guid>` class that includes our custom properties (Bio, Image) and relationships (Following/Followers). Map all User entity properties to ApplicationUser.

**Pros:**
- Standard Identity pattern well-documented by Microsoft
- Maintains all custom properties in the same entity
- EF Core relationships work naturally with single user type
- Follows Identity best practices
- Clean migration path for user-related queries

**Cons:**
- Mixes Identity concerns with domain model
- Need to carefully map between domain User and ApplicationUser
- Following/Followers relationships need to be on ApplicationUser
- May violate clean architecture by putting infrastructure (Identity) concerns in domain

**Impact:** Moderate complexity. Need to create ApplicationUser class and carefully map properties. Need to adjust all user-related queries to use ApplicationUser instead of User. Relationships need to be reconfigured.

### Option B: Separate User Profile Entity

**Description:** Use standard `IdentityUser<Guid>` for authentication and create separate `UserProfile` entity linked by UserId for custom properties (Bio, Image) and relationships.

**Pros:**
- Cleaner separation of authentication vs. profile concerns
- Maintains domain purity (User entity remains domain model)
- Identity changes don't affect domain model
- Easier to test authentication separately from profile logic
- Better alignment with clean architecture principles

**Cons:**
- Need to join two entities for complete user data
- More complex queries (need to include UserProfile)
- Need to maintain synchronization between IdentityUser and UserProfile
- Two entities to manage for every user operation
- Potential for inconsistencies if not carefully managed

**Impact:** Higher complexity. Need to create UserProfile entity and carefully manage synchronization. All queries need to be updated to join or include profile data. More database queries for complete user information.

### Option C: Custom User Store

**Description:** Implement custom `IUserStore<User>` that works directly with our existing User entity without inheriting from IdentityUser.

**Pros:**
- Maximum flexibility - keep existing User entity unchanged
- No need for ApplicationUser or UserProfile
- Existing queries continue to work
- Complete control over data storage

**Cons:**
- Significant implementation effort (need to implement many interfaces)
- Need to implement password storage, security stamps, lockout, etc. manually
- Easy to make security mistakes
- Loses benefits of Identity's built-in features
- Much more code to maintain and test

**Impact:** Very high complexity. Need to implement numerous IUserStore interfaces. Requires deep understanding of Identity internals. Not recommended by Microsoft. High maintenance burden.

### Choice

- [x] Option A: Extend IdentityUser with ApplicationUser
- [ ] Option B: Separate User Profile Entity
- [ ] Option C: Custom User Store

**Rationale:** Option A follows the standard Identity pattern recommended by Microsoft and keeps all user-related data in a single entity, making queries simpler. While it mixes some Identity concerns with domain properties, the benefit of having a single cohesive user entity outweighs the architectural purity of Option B. The migration to ApplicationUser is straightforward - we essentially rename and extend our current User entity. Option C is too complex and error-prone for the benefits it provides.

---

## Decision 3: Identity API Endpoints Integration

Identity provides `MapIdentityApi<TUser>()` which adds JSON API endpoints. We need to decide how to integrate these with our existing FastEndpoints architecture.

### Option A: Use MapIdentityApi Directly

**Description:** Use `MapIdentityApi<ApplicationUser>()` directly and remove existing authentication endpoints. Accept Identity's endpoint structure and update tests to match.

**Pros:**
- Minimal code - leverages built-in endpoints
- Maintained by Microsoft - gets security updates
- Well-tested and battle-hardened
- Standard approach in .NET ecosystem
- Less code to maintain

**Cons:**
- Breaks consistency with FastEndpoints pattern used everywhere else
- Different URL structure than current endpoints (/register vs /api/users)
- Less control over request/response format
- Need to update all tests to match Identity's structure
- May not perfectly match RealWorld API spec (but that's acceptable per requirements)

**Impact:** Low implementation effort. Need to update test suite to use new endpoints. Some URL changes required in frontend.

### Option B: Wrap Identity APIs with FastEndpoints

**Description:** Keep Identity's services but create FastEndpoints wrappers that call `SignInManager` and `UserManager` directly.

**Pros:**
- Maintains FastEndpoints consistency across entire application
- Full control over request/response format
- Can keep existing URL structure if desired
- Easier to customize validation and error handling
- Tests can follow existing patterns

**Cons:**
- More code to maintain
- Need to properly call Identity's SignInManager/UserManager
- Risk of not using Identity correctly
- Duplicates some of Identity's built-in validation
- Requirements explicitly state "use endpoints provided by ASP.NET Identity, not wrappers"

**Impact:** Moderate effort. Need to create FastEndpoints for each auth operation. Goes against stated requirement to use Identity endpoints directly.

### Option C: Hybrid Approach

**Description:** Use `MapIdentityApi` for core operations (register, login, logout, password reset) and FastEndpoints for extended operations (profile update with Bio/Image).

**Pros:**
- Leverages Identity for security-critical operations
- Uses FastEndpoints for domain-specific operations
- Balances standardization with customization
- Gets security updates for auth operations

**Cons:**
- Inconsistent API patterns across application
- More complex to understand for developers
- Need to carefully decide which operations go where
- May confuse API consumers with mixed patterns

**Impact:** Moderate complexity. Need clear documentation on which pattern is used where.

### Choice

- [x] Option A: Use MapIdentityApi Directly
- [ ] Option B: Wrap Identity APIs with FastEndpoints
- [ ] Option C: Hybrid Approach

**Rationale:** The requirements explicitly state "transition to using endpoints provided by ASP.NET Identity, and not try to create wrappers." Option A directly follows this requirement. While it breaks consistency with FastEndpoints, the security and maintenance benefits of using Microsoft's maintained endpoints outweigh the consistency concern. Tests will be updated to match Identity's structure per the requirement that "It's acceptable to revise tests so they match ASP.NET Identity's workflows." Option B directly violates the stated requirement.

---

## Decision 4: Cookie Configuration for SPA

ASP.NET Core Identity supports cookie authentication, which we must use per requirements. We need to configure cookies appropriately for our SPA architecture.

### Option A: Default Cookie Settings with SameSite.Strict

**Description:** Use Identity's default cookie configuration with `SameSite = Strict` for maximum CSRF protection.

**Pros:**
- Maximum security - CSRF protection by default
- Simplest configuration - minimal custom code
- Follows security best practices
- No cross-site concerns for same-origin SPA

**Cons:**
- May cause issues if frontend and backend are on different ports in development
- Cookies not sent with cross-site requests if architecture changes
- May need relaxation if requirements change

**Impact:** Minimal effort. May need to ensure frontend and backend are same origin.

### Option B: SameSite.Lax for Development Flexibility

**Description:** Use `SameSite = Lax` to allow cookies on top-level navigation while still providing CSRF protection.

**Pros:**
- More flexible for development scenarios
- Still provides reasonable CSRF protection
- Allows cookies with GET requests from different origins
- Better for SPA on different port during development

**Cons:**
- Slightly less secure than Strict
- May allow some CSRF vectors (though Identity has other protections)
- Could establish pattern that's later regretted

**Impact:** Minimal effort. Slightly more permissive security posture.

### Option C: Custom Cookie Configuration with Path and Domain

**Description:** Configure cookie with custom path, domain, and SameSite settings optimized for the specific deployment architecture.

**Pros:**
- Maximum control over cookie behavior
- Can optimize for specific deployment scenarios
- Can set different settings for dev vs production

**Cons:**
- More complex configuration
- Need to maintain environment-specific settings
- Easy to misconfigure and create security issues
- May be premature optimization

**Impact:** Moderate effort. Need to research and test cookie settings for each environment.

### Choice

- [x] Option B: SameSite.Lax for Development Flexibility
- [ ] Option A: Default Cookie Settings with SameSite.Strict
- [ ] Option C: Custom Cookie Configuration with Path and Domain

**Rationale:** Option B provides a good balance of security and development flexibility. The application uses SpaProxy for development, which may have the frontend and backend on different ports. `SameSite = Lax` allows the cookies to work in this scenario while still providing reasonable CSRF protection. Identity has additional CSRF protections (anti-forgery tokens) that complement the cookie settings. Option A's Strict setting may cause development friction without significant security benefit given Identity's other protections. Option C is premature optimization before we understand the actual deployment architecture.

---

## Decision 5: Password and Lockout Policy

Identity has configurable password requirements and lockout policies. We need to decide what settings align with security best practices and user experience.

### Option A: Strict Security Policy

**Description:** 
- Password: MinLength=12, RequireDigit=true, RequireNonAlphanumeric=true, RequireUppercase=true, RequireLowercase=true
- Lockout: MaxFailedAttempts=3, LockoutDuration=30 minutes
- RequireConfirmedEmail=true

**Pros:**
- Maximum security posture
- Protects against brute force attacks
- Forces strong passwords
- Email confirmation prevents fake accounts

**Cons:**
- May frustrate users (high password complexity)
- Email confirmation adds complexity (need email service)
- Aggressive lockout may frustrate legitimate users
- Higher support burden for locked accounts

**Impact:** Moderate implementation (need email service). May require user education.

### Option B: Balanced Security Policy

**Description:**
- Password: MinLength=8, RequireDigit=true, RequireNonAlphanumeric=false, RequireUppercase=true, RequireLowercase=true  
- Lockout: MaxFailedAttempts=5, LockoutDuration=10 minutes
- RequireConfirmedEmail=false (for development phase)

**Pros:**
- Reasonable security without excessive friction
- Standard lockout policy (5 attempts common)
- No email service needed initially
- Easier for testing and development
- Can tighten later as needed

**Cons:**
- Less secure than Option A
- Passwords could be weaker (8 chars, no special chars required)
- Fake accounts possible without email confirmation
- May need migration to stricter policy later

**Impact:** Low implementation effort. Can be configured without email service.

### Option C: Minimal Security Policy (Development Only)

**Description:**
- Password: MinLength=6 (matching current requirement), minimal other requirements
- Lockout: Disabled
- RequireConfirmedEmail=false

**Pros:**
- Easiest for development and testing
- Maintains current password minimum
- No lockout to worry about during development

**Cons:**
- Not production-ready
- Vulnerable to brute force
- Weak passwords allowed
- Need to remember to tighten before production

**Impact:** Minimal effort but creates technical debt.

### Choice

- [x] Option B: Balanced Security Policy
- [ ] Option A: Strict Security Policy
- [ ] Option C: Minimal Security Policy

**Rationale:** Option B provides reasonable security without requiring an email service infrastructure that's not yet in place. The 8-character minimum is standard, and requiring digit/upper/lower is reasonable without being overly burdensome. The 5-attempt, 10-minute lockout is industry standard. Email confirmation can be added in a later phase when email infrastructure is ready. Option A is too strict for current development phase (email service not ready). Option C is too permissive and creates technical debt.

---

## Decision 6: IUserContext Abstraction Layer

The codebase has an `IUserContext` abstraction that use cases depend on. Identity changes the underlying authentication mechanism, requiring a decision on this abstraction.

### Option A: Keep IUserContext, Update Implementation

**Description:** Maintain the `IUserContext` interface unchanged and update `UserContext` implementation to extract claims from Identity's cookie authentication instead of JWT.

**Pros:**
- Zero changes required in use cases/handlers
- Maintains clean architecture separation
- Use cases remain testable and framework-agnostic
- Identity implementation detail hidden from business logic

**Cons:**
- May need to adjust claim extraction logic
- GetCurrentToken() method may be less relevant with cookies
- Implementation more complex than directly accessing User

**Impact:** Low effort. Only need to update UserContext implementation. No changes to consuming code.

### Option B: Remove IUserContext, Use HttpContext.User Directly

**Description:** Remove the abstraction and have handlers access `HttpContext.User` directly through FastEndpoints' `User` property.

**Pros:**
- Simpler - fewer layers of indirection
- More standard ASP.NET Core pattern
- Less code to maintain
- Direct access to all claims

**Cons:**
- Couples use cases to ASP.NET Core (violates clean architecture)
- Need to update all handlers that use IUserContext
- Harder to test (need to mock HttpContext)
- Loses business-friendly interface (GetRequiredCurrentUserId vs claim parsing)

**Impact:** High effort. Need to update many handler classes. Reduces testability.

### Option C: Extend IUserContext with Identity-Specific Methods

**Description:** Keep IUserContext but add new methods for Identity-specific operations (GetRoles, GetClaims, etc.) while keeping existing methods.

**Pros:**
- Backwards compatible with existing code
- Allows gradual migration to Identity features
- Can expose Identity capabilities when needed

**Cons:**
- Interface grows larger over time
- May leak Identity concepts into abstraction
- More methods to implement and test

**Impact:** Moderate effort. Need to extend interface and implementation carefully.

### Choice

- [x] Option A: Keep IUserContext, Update Implementation
- [ ] Option B: Remove IUserContext, Use HttpContext.User Directly
- [ ] Option C: Extend IUserContext with Identity-Specific Methods

**Rationale:** Option A maintains the clean architecture principles that are emphasized throughout the codebase. The IUserContext abstraction serves an important purpose - it keeps use cases testable and independent of framework concerns. The existing interface provides exactly what business logic needs without exposing authentication implementation details. Updating the implementation to work with Identity cookies instead of JWT is straightforward. Option B would violate clean architecture and reduce testability. Option C is premature - we should only extend the interface if we actually need Identity-specific operations in use cases.

---

## Summary of Key Decisions

1. **Audit.NET Integration**: Use `AuditIdentityDbContext` from `Audit.EntityFramework.Identity` package
2. **User Entity**: Extend IdentityUser with `ApplicationUser` class containing custom properties
3. **API Endpoints**: Use `MapIdentityApi<ApplicationUser>()` directly, not FastEndpoints wrappers
4. **Cookie Settings**: Use `SameSite = Lax` for development flexibility
5. **Security Policy**: Balanced policy (8-char min, 5-attempt lockout, no email confirmation initially)
6. **IUserContext**: Keep abstraction, update implementation for Identity

These decisions provide a clear path forward for the migration while balancing security, maintainability, and development velocity.