## phase_2: Remove Unauthenticated Data Access from Frontend

### Phase Overview

Update the React frontend to require authentication for all data viewing. Remove global feed, article detail, and profile viewing for unauthenticated users. This prepares the application for multi-tenancy where query filters will make unauthenticated data access impossible.

**Scope Size:** Small (~8 steps)
**Risk Level:** Low (frontend-only changes, no backend impact)
**Estimated Complexity:** Low

### Prerequisites

What must be completed before starting this phase:
- Phase 1 (POC) completed and validated
- Decision 1 from key-decisions.md confirmed viable
- All existing tests passing (45 functional, 51 E2E, 5 Postman collections)

### Known Risks & Mitigations

**Risk 1:** Frontend tests may assume unauthenticated browsing is possible
- **Likelihood:** High
- **Impact:** Medium (test failures, need updates)
- **Mitigation:** Update frontend tests incrementally as routes are changed. Use AuthContext.Provider with mock user in tests.
- **Fallback:** If tests are too coupled to unauthenticated browsing, add temporary "stub" login to test setup

**Risk 2:** E2E tests may rely on unauthenticated browsing flows
- **Likelihood:** High
- **Impact:** Medium (E2E test failures)
- **Mitigation:** Update E2E tests to login before accessing protected routes. Database wipe scripts remain unchanged in this phase.
- **Fallback:** Skip E2E tests temporarily (document), fix in phase 3 when backend is also updated

### Implementation Steps

**Part 1: Update Routing Configuration**

1. **Make home route require authentication**
   - Update routing configuration in `App/Client/src/main.tsx` or router file
   - Redirect unauthenticated users from `/` to `/login`
   - Expected outcome: Unauthenticated users cannot access home page
   - Files affected: `App/Client/src/main.tsx`
   - Reality check: Navigate to `/` without login, should redirect to `/login`

2. **Protect article and profile routes**
   - Add authentication guards to `/article/:slug` and `/profile/:username` routes
   - Redirect to `/login` if not authenticated
   - Expected outcome: Article detail and profile pages require auth
   - Files affected: `App/Client/src/main.tsx` or route definitions
   - Reality check: Try accessing article URL without auth, should redirect

**Part 2: Update Home Page Component**

3. **Remove global feed view for unauthenticated users**
   - Update Home page component (`App/Client/src/pages/Home.tsx` or equivalent)
   - Remove code path that displays global feed for unauthenticated users
   - Show only "Your Feed" (which requires authentication)
   - Expected outcome: Home page only renders for authenticated users
   - Files affected: `App/Client/src/pages/Home.tsx`
   - Reality check: Home component doesn't try to fetch global feed without auth

4. **Update navigation components**
   - Update header/navigation (`App/Client/src/components/Header.tsx` or similar)
   - Remove "Home" link for unauthenticated users (since home now requires auth)
   - Ensure login/register links are prominent for unauthenticated users
   - Expected outcome: Navigation reflects authentication-required state
   - Files affected: `App/Client/src/components/Header.tsx`, `App/Client/src/components/Navigation.tsx`

**Part 3: Update Frontend Tests**

5. **Update component tests for auth-required behavior**
   - Find frontend tests that render Home, ArticlePage, ProfilePage components
   - Wrap test renders with AuthContext.Provider providing mock authenticated user
   - Expected outcome: Component tests pass with authenticated context
   - Files affected: `App/Client/src/pages/*.test.tsx`, `App/Client/src/components/*.test.tsx`
   - Reality check: Run `./build.sh TestClient`, all tests pass

6. **Update route tests**
   - Update tests that navigate to protected routes
   - Add authentication to test setup (mock login or AuthContext)
   - Expected outcome: Route tests work with authentication requirement
   - Files affected: `App/Client/src/*.test.tsx`
   - Reality check: Tests no longer fail due to missing auth

**Part 4: Update E2E Tests**

7. **Update E2E tests to login before browsing**
   - Find E2E tests that browse articles or profiles without login
   - Add login step before accessing protected content
   - Expected outcome: E2E tests authenticate before accessing data
   - Files affected: `Test/e2e/E2eTests/*HappyPathTests.cs`
   - Reality check: Run `./build.sh TestE2e`, tests pass with login

8. **Update E2E home page tests**
   - Update tests that assume unauthenticated home page access
   - Add authentication before navigating to home
   - Expected outcome: Home page E2E tests work with auth requirement
   - Files affected: `Test/e2e/E2eTests/HomePage/*Tests.cs`
   - Reality check: Home page tests pass

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After routing changes
./build.sh LintClientVerify
./build.sh BuildClient
./build.sh TestClient

# After component updates
./build.sh TestClient

# After E2E test updates
./build.sh TestE2e
```

Don't wait until the end to test. Reality test after each major change.

### Expected Working State After Phase

When this phase is complete:
- Frontend redirects unauthenticated users to login page
- Home page, article detail, profile pages all require authentication
- No unauthenticated data viewing possible in frontend
- All frontend tests pass with authentication in place
- E2E tests pass with login step before browsing
- **Backend unchanged** - still allows unauthenticated access (fixed in phase 3)
- Frontend and backend still compatible (backend allows anonymous, frontend just doesn't use it)

### If Phase Fails

If this phase fails and cannot be completed:
1. Check for frontend tests that are tightly coupled to unauthenticated browsing
2. Use docfork MCP server to search React Router and React Context patterns for authentication guards
3. Try debug-analysis.md if tests fail unexpectedly
4. If E2E tests are too difficult to update now, document and defer to phase 3 (update E2E and backend together)

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintClientVerify
./build.sh BuildClient  
./build.sh TestClient
./build.sh TestE2e
```

All targets must pass before proceeding to the next phase.

**Manual Verification Steps:**
1. Start the application (`./build.sh RunLocalPublish`)
2. Navigate to the backend HTTPS URL (the App/Server application serves static frontend assets, and SPA proxy forwards to vite dev server)
3. Verify redirect to login page when accessing root without logging in
4. Try to access `/article/some-slug` directly - should redirect to login
5. Login and verify home page, articles, profiles are accessible
6. Logout and verify redirect back to login page