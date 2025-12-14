## phase_4

### Phase Overview

**[UPDATED: Option A - Single Source of Truth]**

Completely migrate from legacy User entity to ApplicationUser (Identity) as the single source of truth. This phase removes dual-write complexity by updating ALL handlers that reference User to use ApplicationUser via UserManager. This is an aggressive but clean migration that eliminates the legacy User table entirely in favor of Identity's AspNetUsers table.

### Prerequisites

- Phase 3 completed: Identity API endpoints available via MapIdentityApi at /api/identity
- Both authentication systems working independently
- Manual testing confirms Identity endpoints work correctly

### Migration Strategy

**Option A Approach:**
- Remove ALL uses of IRepository<User>
- Migrate ALL handlers to use UserManager<ApplicationUser>
- Update JWT token generation to work with ApplicationUser
- Remove legacy User entity after all migrations complete
- No dual-write - single source of truth in AspNetUsers table

### Implementation Steps

**Part 1: Core User Endpoints (Register, Login, GetCurrent, Update)**

1. **Update Register Endpoint to Use UserManager (NO Dual-Write)**
   - Remove IRepository<User> injection
   - Use only UserManager<ApplicationUser>
   - Return ApplicationUser directly (no MapToLegacyUser)
   - Update mapper to work with ApplicationUser for JWT generation

2. **Update Login Endpoint to Use SignInManager**
   - Remove IRepository<User> and IPasswordHasher
   - Use only UserManager and SignInManager
   - Return ApplicationUser directly

3. **Update GetCurrent Endpoint**
   - Remove IRepository<User>
   - Use only UserManager<ApplicationUser>
   - Query by ID using UserManager.FindByIdAsync()

4. **Update UpdateUser Endpoint**
   - Remove IRepository<User>
   - Use only UserManager<ApplicationUser>
   - Update via UserManager.UpdateAsync()
   - Use password reset tokens for password changes

**Part 2: Update JWT Token Generation**

5. **Update IJwtTokenGenerator and JwtTokenGenerator**
   - Change interface from `GenerateToken(User user)` to `GenerateToken(ApplicationUser user)`
   - Update implementation to extract claims from ApplicationUser
   - Ensure Id, Email, Username are correctly mapped

6. **Update UserMapper**
   - Change FromEntity parameter from User to ApplicationUser
   - Update property mappings (Email, Username, Bio, Image)
   - Ensure JWT token generation works with ApplicationUser

**Part 3: Migrate Article Handlers**

7. **Update CreateArticleHandler**
   - Replace IRepository<User> with UserManager<ApplicationUser>
   - Update user lookup to use UserManager.FindByIdAsync()

8. **Update FavoriteArticleHandler**
   - Replace IRepository<User> with UserManager<ApplicationUser>
   - Update user lookup logic

9. **Update UnfavoriteArticleHandler**
   - Replace IRepository<User> with UserManager<ApplicationUser>
   - Update user lookup logic

**Part 4: Migrate Comment Handlers**

10. **Update CreateCommentHandler**
    - Replace IRepository<User> with UserManager<ApplicationUser>
    - Update author lookup to use UserManager

11. **Update GetCommentsHandler**
    - Replace IRepository<User> with UserManager<ApplicationUser>
    - Update user lookups for author information

**Part 5: Migrate Profile Handlers**

12. **Update GetProfileHandler**
    - Replace IRepository<User> with UserManager<ApplicationUser>
    - Update user query logic

13. **Update FollowUserHandler**
    - Replace IRepository<User> with UserManager<ApplicationUser>
    - Update follower/following logic to work with ApplicationUser

14. **Update UnfollowUserHandler**
    - Replace IRepository<User> with UserManager<ApplicationUser>
    - Update follower/following logic

**Part 6: Update Mappers**

15. **Update ArticleMapper**
    - Change user repository lookups to use UserManager
    - Update to work with ApplicationUser

16. **Update ProfileMapper**
    - Change user repository lookups to use UserManager
    - Update to work with ApplicationUser

**Part 7: Enable Cookie Authentication**

17. **Enable Cookie Authentication for FastEndpoints**
    - Update GetCurrent and UpdateUser endpoints to accept both "Token" and Cookie schemes
    - Update Article/Profile/Comment endpoints if they need auth

**Part 8: Remove Legacy Code**

18. **Remove Legacy User Entity and Related Code**
    - Remove `Server.Core/UserAggregate/User.cs`
    - Remove `Server.Core/UserAggregate/UserFollowing.cs` (migrate to ApplicationUser)
    - Remove User specifications (UserByEmailSpec, UserByUsernameSpec)
    - Remove IPasswordHasher interface and BcryptPasswordHasher implementation
    - Remove User entity configuration from EF Core
    - Remove DbSet<User> from AppDbContext
    - Create migration to drop Users table

**Part 9: Fix Following/Followers Relationships**

19. **Update Following/Followers for ApplicationUser**
    - The ApplicationUser already has Following and Followers collections
    - Ensure UserFollowing references ApplicationUser, not User
    - Update UserFollowing entity configuration
    - Create migration to update foreign keys

**Part 10: Testing**

20. **Update Test Fixtures**
    - Update fixtures to use UserManager for user creation
    - Add cookie support where needed

21. **Run and Fix Tests**
    - Run `./build.sh TestServer`
    - Run `./build.sh TestServerPostman`
    - Run `./build.sh TestE2e`
    - Fix any failures

### Verification
      - Cookie handling in tests
      - Database queries expecting legacy User entity
    - Ensure no regressions in existing tests

12. **Verify Dual Authentication Works**
    - Manually test the complete flows:
      - Register via /api/users -> login via /api/identity
      - Register via /api/identity -> login via /api/users
      - Access protected endpoints with both auth types
    - Verify only AspNetUsers table is used (no legacy Users table)
    - Verify no data loss or issues with user operations

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh TestServer
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass. The existing /api/users endpoints now use ASP.NET Identity internally (UserManager and SignInManager) while maintaining backward compatibility. Functional tests validate cross-authentication scenarios between /api/users and /api/identity endpoints. Postman and E2E tests continue to work with /api/users endpoints without changes.