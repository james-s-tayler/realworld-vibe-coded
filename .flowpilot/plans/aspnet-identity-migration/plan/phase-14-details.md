## phase_14

### Phase Overview

Update the remaining postman collections (Profiles, FeedAndArticles, Article, ArticlesEmpty) to switch to calling /api/identity/register and /api/identity/login endpoints using the Identity bearer token scheme. After this phase, all postman tests validate Identity endpoints.

### Prerequisites

- Phase 13 completed: Auth collection using Identity endpoints successfully
- Backend supports three authentication schemes
- Other collections still using /api/users endpoints

### Implementation Steps

1. **Update Profiles Postman Collection**
   - Open `Test/Postman/Conduit.Profiles.postman_collection.json`
   - Update all register requests to use /api/identity/register
   - Update all login requests to use /api/identity/login
   - Update token extraction to use accessToken field
   - Update authorization headers to use "Authorization: Bearer {{token}}"
   - Update any test assertions related to authentication

2. **Test Profiles Collection**
   - Run `./build.sh TestServerPostmanProfiles`
   - Verify all tests pass with Identity endpoints
   - Fix any failures

3. **Update FeedAndArticles Postman Collection**
   - Open `Test/Postman/Conduit.FeedAndArticles.postman_collection.json`
   - Update all register requests to use /api/identity/register
   - Update all login requests to use /api/identity/login
   - Update token extraction to use accessToken field
   - Update authorization headers to use "Authorization: Bearer {{token}}"
   - Update any test assertions related to authentication

4. **Test FeedAndArticles Collection**
   - Run `./build.sh TestServerPostmanFeedAndArticles`
   - Verify all tests pass with Identity endpoints
   - Fix any failures

5. **Update Article Postman Collection**
   - Open `Test/Postman/Conduit.Article.postman_collection.json`
   - Update all register requests to use /api/identity/register
   - Update all login requests to use /api/identity/login
   - Update token extraction to use accessToken field
   - Update authorization headers to use "Authorization: Bearer {{token}}"
   - Update any test assertions related to authentication
   - This is the largest collection so may require more work

6. **Test Article Collection**
   - Run `./build.sh TestServerPostmanArticle`
   - Verify all tests pass with Identity endpoints
   - Fix any failures

7. **Update ArticlesEmpty Postman Collection**
   - Open `Test/Postman/Conduit.ArticlesEmpty.postman_collection.json`
   - Update all register requests to use /api/identity/register (if any)
   - Update all login requests to use /api/identity/login (if any)
   - Update token extraction to use accessToken field (if any)
   - Update authorization headers to use "Authorization: Bearer {{token}}" (if any)
   - Update any test assertions related to authentication (if any)

8. **Test ArticlesEmpty Collection**
   - Run `./build.sh TestServerPostmanArticlesEmpty`
   - Verify all tests pass
   - Fix any failures

9. **Verify All Postman Collections Together**
   - Run `./build.sh TestServerPostman`
   - All collections should pass
   - All collections now use Identity endpoints exclusively

10. **Verify E2E Tests Still Pass**
    - Run `./build.sh TestE2e`
    - E2E tests still use /api/users endpoints (not yet migrated)
    - They should continue to pass

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostmanAuth
./build.sh TestServerPostmanProfiles
./build.sh TestServerPostmanFeedAndArticles
./build.sh TestServerPostmanArticle
./build.sh TestServerPostmanArticlesEmpty
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass. All postman collections should now use Identity endpoints with Bearer token authentication. Frontend and E2E tests still use /api/users endpoints. Backend supports all authentication schemes.
