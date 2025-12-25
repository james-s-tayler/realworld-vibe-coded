## phase_3: Require Authentication on All Backend Endpoints

### Phase Overview

Update all backend GET endpoints (articles, comments, profiles, tags) to require authentication. Remove support for anonymous/unauthenticated access. Update Postman collections and functional tests to expect 401 Unauthorized when accessed without auth tokens.

**Scope Size:** Small (~7 steps)
**Risk Level:** Medium (breaking API change, affects all clients)
**Estimated Complexity:** Medium

### Prerequisites

What must be completed before starting this phase:
- Phase 2 completed (frontend requires authentication)
- Frontend tests passing with authentication
- All E2E tests updated for authentication in phase 2

### Known Risks & Mitigations

**Risk 1:** Functional tests may assume anonymous access to GET endpoints
- **Likelihood:** High
- **Impact:** High (many test failures)
- **Mitigation:** Update all functional test fixtures to create authenticated HttpClient instances with auth tokens
- **Fallback:** Temporarily skip failing tests, fix incrementally

**Risk 2:** Postman collections may not have auth configured for GET requests
- **Likelihood:** High
- **Impact:** Medium (Postman tests fail)
- **Mitigation:** Update Postman collections to include auth tokens for all requests. Use collection-level auth variables.
- **Fallback:** Update pre-request scripts to login and capture token before each request

### Implementation Steps

**Part 1: Update FastEndpoints Configuration**

1. **Identify all GET endpoints**
   - Use grep to find all GET endpoints: `grep -r "Get<" App/Server/src/Server.Web/Endpoints`
   - List includes: GET /api/articles, GET /api/articles/:slug, GET /api/articles/:slug/comments, GET /api/profiles/:username, GET /api/tags
   - Expected outcome: Complete list of endpoints to update
   - Reality check: Count should match system-analysis.md (5-7 GET endpoints)

2. **Remove AllowAnonymous from endpoints**
   - Update endpoint classes to remove `AllowAnonymous()` calls or add `AuthSchemes()` requirement
   - For FastEndpoints, use `.AuthSchemes("Bearer", "Cookie")` or remove AllowAnonymous policy
   - Expected outcome: Endpoints require authentication
   - Files affected: `App/Server/src/Server.Web/Endpoints/Articles/*.cs`, `App/Server/src/Server.Web/Endpoints/Profiles/*.cs`, `App/Server/src/Server.Web/Endpoints/Tags/*.cs`
   - Reality check: Build succeeds, no compilation errors

**Part 2: Update Functional Tests**

3. **Update test fixtures to create authenticated clients**
   - Modify ArticlesFixture, ProfilesFixture, TagsFixture in Server.FunctionalTests
   - Add helper method to create authenticated HttpClient with Bearer token
   - Use FastEndpoints.Testing pattern: `await Client.POSTAsync<Login, LoginRequest, LoginResponse>(loginRequest)` to get token
   - Expected outcome: Test fixtures provide authenticated clients
   - Files affected: `App/Server/tests/Server.FunctionalTests/Fixtures/*.cs`
   - Reality check: Test setup methods compile

4. **Update GET endpoint tests**
   - Update tests that call GET /api/articles, GET /api/profiles/:username, GET /api/tags
   - Replace anonymous client with authenticated client from fixture
   - Expected outcome: Tests use authenticated clients
   - Files affected: `App/Server/tests/Server.FunctionalTests/Articles/*Tests.cs`, `App/Server/tests/Server.FunctionalTests/Profiles/*Tests.cs`, `App/Server/tests/Server.FunctionalTests/Tags/*Tests.cs`
   - Reality check: Run `./build.sh TestServer`, tests pass

5. **Add 401 Unauthorized tests**
   - Create new tests that call GET endpoints without auth token
   - Verify response is 401 Unauthorized
   - Expected outcome: Comprehensive coverage of auth requirement
   - Files affected: `App/Server/tests/Server.FunctionalTests/Articles/ArticlesPermissionsTests.cs` (new or existing)
   - Reality check: 401 tests pass

**Part 3: Update Postman Collections**

6. **Update unauthenticated test expectations**
   - Bearer token auth is already configured in existing Postman collections
   - Find tests that currently test unauthenticated GET requests
   - Update these tests to expect 401 Unauthorized instead of 200 OK
   - Expected outcome: Tests that verify unauthenticated behavior now expect 401
   - Files affected: `Test/Postman/*.postman_collection.json`
   - Reality check: Run individual Postman targets: `./build.sh TestServerPostmanArticlesEmpty`, `./build.sh TestServerPostmanAuth`, etc.

**Part 4: Manual Testing**

7. **Test with Postman manually**
   - Run individual Postman collection targets to verify auth behavior
   - Test requests without auth should now return 401
   - Test requests with auth should return 200 OK
   - Expected outcome: Manual confirmation of auth enforcement
   - Reality check: Anonymous requests return 401

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After endpoint changes
./build.sh LintServerVerify
./build.sh BuildServer

# After functional test updates
./build.sh TestServer

# After Postman updates (test individual collections)
./build.sh TestServerPostmanArticlesEmpty
./build.sh TestServerPostmanAuth
./build.sh TestServerPostmanProfiles
./build.sh TestServerPostmanFeedAndArticles
./build.sh TestServerPostmanArticle

# Verify E2E tests still pass (should already authenticate from phase 2)
./build.sh TestE2e
```

Don't wait until the end to test. Reality test after each major change.

### Expected Working State After Phase

When this phase is complete:
- All GET endpoints require authentication (Bearer or Cookie)
- Anonymous requests to GET endpoints return 401 Unauthorized
- Functional tests pass with authenticated clients
- Postman collections pass with updated expectations for unauthenticated tests
- E2E tests pass (already authenticate from phase 2)
- Backend now requires authentication for all data access
- System ready for multi-tenancy (query filters won't break unauthenticated access)

### If Phase Fails

If this phase fails and cannot be completed:
1. Check FastEndpoints documentation for auth configuration using docfork MCP server
2. Review functional test auth patterns in existing tests (e.g., login/register tests)
3. Use debug-analysis.md if test failures are unclear
4. Check Postman collection auth configuration - may need collection-level vs request-level auth
5. If Postman auth is too complex, temporarily skip Postman tests and fix in follow-up

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostmanArticlesEmpty
./build.sh TestServerPostmanAuth
./build.sh TestServerPostmanProfiles
./build.sh TestServerPostmanFeedAndArticles
./build.sh TestServerPostmanArticle
./build.sh TestE2e
```

All targets must pass before proceeding to the next phase.

**Manual Verification Steps:**
1. Start application: `./build.sh RunLocalPublish`
2. Use curl or Postman to test GET /api/articles without auth - expect 401
3. Check server logs for no errors
4. Verify all Postman collection targets pass
