# Debug Analysis - Phase 5 Postman Tests (Iteration 2)

Current Status: **89% passing (412/461 tests)**
Goal: **100% passing**

## Summary of Remaining 49 Failures

Based on ReportSummary.md, failures fall into clear categories:

1. **Update Article returning empty object** (8 related failures)
2. **Follow/Unfollow Profile operations** (12 failures)  
3. **Feed and Articles by author queries returning 0 results** (14 failures)
4. **Authorization status code mismatches** (4 failures)
5. **Comment and Delete operations** (4 failures)
6. **Miscellaneous** (7 failures)

---

## 1. Problem Definition & Scope

- **What is the exact observable behavior that is incorrect?**
  - Analysis: Multiple distinct issues: (1) Update Article returns `{}` instead of `{article: {...}}`, (2) Follow operations return 404 Not Found instead of 200 OK, (3) Feed queries return 0 articles instead of expected counts, (4) Authorization tests expect 403 Forbidden but get 204/200/404

- **What is the expected behavior, stated concretely?**
  - Analysis: (1) Update Article should return `{article: {title, slug, body, ...}}`, (2) Follow/Unfollow should return `{profile: {username, bio, image, following}}`, (3) Feed should return articles from followed users, (4) Unauthorized operations should return 403 Forbidden

- **What is the smallest, most precise statement of the problem?**
  - Analysis: **Three distinct root causes**: (1) Update Article endpoint may not be returning data with cookie auth, (2) Profile follow operations failing with 404 suggests users or relationships don't exist, (3) Feed returning 0 articles suggests follow relationships aren't being created

- **What is the scope of the failure?**
  - Analysis: Specific to: Update Article endpoint, Profile Follow/Unfollow operations, and Feed queries that depend on follow relationships. Not global - other operations work fine (Create Article, Get Profile, etc.)

- **What would definitively prove that the problem is fixed?**
  - Analysis: All 461 Postman tests passing (100%)

---

## 2. Reproduction & Minimization

- **Can the problem be reproduced reliably?**
  - Analysis: **YES** - 100% reproducible. Same 49 tests fail every run.

- **What is the minimal input that triggers the problem?**
  - Analysis: For Update Article: PUT request with valid article data + cookie auth. For Follow Profile: POST to /api/profiles/{email}/follow with cookie auth. For Feed: GET /api/articles/feed with cookie auth after following users.

- **What variables can be removed?**
  - Analysis: Problems occur with minimal setup - just Register, Login, and the specific operation. No complex dependencies.

- **Can the issue be reproduced in isolation?**
  - Analysis: **YES** - Can test individual operations via curl/Postman GUI with cookie auth

- **What is the simplest system version where this fails?**
  - Analysis: Current system with cookie authentication. Functional tests (217/217) all pass, which test same API operations but with different setup

---

## 3. Search Space Reduction

- **What single question eliminates half the hypotheses?**
  - Analysis: **"Does Update Article work correctly when called manually with cookie auth?"** If YES → Postman test setup issue. If NO → API endpoint bug.

- **Where is the earliest divergence point?**
  - Analysis: **After Login succeeds but before the actual operation**. The cookie must be present but something about the request/response is different.

- **Is the problem in input, transformation, or output?**
  - Analysis: **Output** for Update Article (wrong response format), **State/Data** for Follow/Feed (operations depend on data that may not exist)

- **What invariant is violated?**
  - Analysis: (1) "Update Article always returns article data", (2) "Follow operations create follow relationships", (3) "Feed returns articles from followed users"

---

## 4. Assumptions & Ground Truth

- **What assumptions have I not verified?**
  - Analysis: 
    1. ❌ **UNVERIFIED**: Does Update Article actually return data with cookie auth?
    2. ❌ **UNVERIFIED**: Are Follow Profile operations actually creating follow relationships in the database?
    3. ❌ **UNVERIFIED**: Does the API actually use the email format for profile usernames (celeb_email@domain)?
    4. ✅ **VERIFIED**: Login creates authentication cookies (tests improved from 44% to 89% after adding Login calls)

- **What do I know for certain?**
  - Analysis:
    1. Functional tests pass 100% (217/217) - API logic is correct
    2. E2E tests pass 100% - Full user workflows work
    3. Postman tests went from 44% → 89% after fixing authentication
    4. All 126 API requests succeed (no 500 errors, network issues)

- **Am I debugging the correct system?**
  - Analysis: **YES** - Same codebase, same database, same setup as functional/E2E tests

- **Could I be observing wrong logs?**
  - Analysis: **NO** - Logs in Logs/Server.Web/Serilog/logs20251215.txt are from the test run

---

## 5. Observability & Instrumentation

- **What logs are available?**
  - Analysis: Serilog logs in `Logs/Server.Web/Serilog/logs20251215.txt` (1.1 MB), Audit.NET logs for database transactions

- **What additional logging would clarify behavior?**
  - Analysis: **HIGH VALUE**: Log the actual HTTP response body for Update Article requests. Log whether Follow operations successfully insert database records. Log Feed query SQL and results.

- **Are correlation IDs available?**
  - Analysis: **YES** - Serilog structured logging includes request context

- **What should be logged but isn't?**
  - Analysis: Response body content for failed assertions, actual vs expected follow relationship state, Feed query WHERE clause and result count

---

## 6. Inputs, Outputs & Boundaries

- **Are all inputs exactly what I believe?**
  - Analysis: **NEEDS VERIFICATION** - Profile URLs use `celeb_{{EMAIL}}` format. Need to verify this matches actual username in database. Could be format mismatch (e.g., URL has celeb_soloyolo@mail.com but DB has soloyolo@mail.com)

- **Are there null, empty, or boundary values?**
  - Analysis: Update Article response is literally `{}` - suggests endpoint returns object with no properties. Could be mapping/serialization issue.

- **Could serialization alter data?**
  - Analysis: **LIKELY FOR UPDATE ARTICLE** - Response format is wrong (`{}` vs `{article: {...}}`). This suggests different response model or serialization path for cookie auth vs JWT.

- **Is implicit conversion occurring?**
  - Analysis: Profile usernames: tests expect string email addresses, need to verify exact format stored and queried

---

## 7. State, Caching & Persistence

- **What state persists across executions?**
  - Analysis: Database persists between folders. Each folder creates its own users (EMAIL variable is unique per folder). Follow relationships created in one folder don't carry to others.

- **Could cached data cause issues?**
  - Analysis: **UNLIKELY** - Fresh database, no HTTP caching on API

- **Have all caches been cleared?**
  - Analysis: **YES** - Docker containers rebuilt, fresh database each run

- **Is system starting clean?**
  - Analysis: **YES** - DbResetForce target runs before tests

- **Could previous failures leave corrupted state?**
  - Analysis: **NO** - Fresh start each run

---

## 8. Time, Ordering & Concurrency

- **Does behavior change with reduced concurrency?**
  - Analysis: **N/A** - Newman runs tests sequentially

- **Are there ordering assumptions?**
  - Analysis: **YES - CRITICAL INSIGHT**: Feed tests expect articles from followed users. If Follow operations fail (which they are - 404 Not Found), then Feed will return 0 articles. **This explains cascading failures**.

- **Could this be a race condition?**
  - Analysis: **NO** - Sequential execution

- **Are retries or async involved?**
  - Analysis: **NO** - Synchronous HTTP requests

---

## 9. Configuration & Environment

- **What configuration influences behavior?**
  - Analysis: Cookie authentication (`useCookies=true`), HTTPS URLs, EMAIL variable format

- **Are there environment differences?**
  - Analysis: **NEED TO CHECK**: Functional tests may use different test data setup than Postman tests

- **Are versions as expected?**
  - Analysis: **YES** - .NET 9, same code for all test suites

---

## 10. Dependencies & External Systems

- **What external dependencies exist?**
  - Analysis: SQL Server database, HTTPS certificate

- **Could dependency misbehave?**
  - Analysis: **NO** - Same database works for functional tests

- **Can dependency be isolated?**
  - Analysis: **YES** - Can query database directly to verify state

---

## 11. Data Integrity & Invariants

- **What invariants should hold?**
  - Analysis:
    1. After Follow operation succeeds (200 OK), follow relationship exists in database
    2. After Create Article, article exists and is queryable
    3. Update Article returns same article with updated fields

- **Is data complete and valid?**
  - Analysis: **NEEDS VERIFICATION** - Are follow relationships actually being created?

- **Could there be poison data?**
  - Analysis: **POSSIBLE**: Username format mismatch (celeb_email@mail.com vs email@mail.com)

- **Is there schema mismatch?**
  - Analysis: **NO** - Same schema for all tests

---

## 12. Control Flow & Dispatch

- **Am I certain the code path is executing?**
  - Analysis: **NEEDS VERIFICATION** for Update Article - might have different endpoint or handler for cookie auth

- **Is error handling masking failures?**
  - Analysis: **LIKELY** - Follow operations return 404 Not Found, which suggests "user not found" error being caught and returned as 404 instead of creating relationship

---

## 13. Build, Packaging & Deployment

- **Was system rebuilt?**
  - Analysis: **YES** - BuildServerPublish target runs before tests

- **Correct artifact deployed?**
  - Analysis: **YES** - Same build for all tests

---

## 14. Security, Permissions & Identity

- **Could this be auth/authz issue?**
  - Analysis: **PARTIALLY** - Authorization status code mismatches (expecting 403, getting 204/200/404) suggest different error handling for cookie auth

- **Does behavior differ by user/role?**
  - Analysis: **N/A** - All users are standard users

---

## 15. Cognitive Bias & Debugging Traps

- **What belief am I most confident in?**
  - Analysis: "The API works correctly because functional tests pass." This is TRUE but doesn't mean response format is same for cookie auth vs JWT.

- **Am I anchored on misleading error?**
  - Analysis: **POSSIBLY** - "404 Not Found" for Follow Profile makes me think user doesn't exist, but could be different issue (wrong URL format, etc.)

- **Am I debugging symptoms vs root cause?**
  - Analysis: **YES** - Need to stop fixing individual tests and identify the pattern:
    1. **Update Article** → Response format issue
    2. **Follow Profile** → User lookup/URL format issue  
    3. **Feed** → Cascading failure from Follow not working

- **Am I avoiding simple explanation?**
  - Analysis: **POSSIBLY** - Simple explanation: The celeb_{{EMAIL}} format in URLs doesn't match actual usernames in database

---

## 16. Comparison & History

- **Is there a known-good version?**
  - Analysis: **YES** - Functional tests use same API but different test setup

- **What differences exist?**
  - Analysis: Functional tests create users with `CreateUser` helper and known usernames. Postman tests use Identity API register/login with email-based usernames.

- **Can I bisect to find introduction point?**
  - Analysis: **NOT APPLICABLE** - Not a regression, this is migration to new auth system

---

## 17. Hypothesis Testing

- **Current leading hypotheses:**
  1. **Update Article returns wrong response format with cookie auth** - Needs API testing
  2. **Profile URLs use wrong username format** - `celeb_{{EMAIL}}` doesn't match database username
  3. **Follow operations fail, causing Feed to return 0 articles** - Cascading failure

- **Most valuable experiment:**
  - Analysis: **Test Follow Profile operation manually via curl with cookie auth**
    ```bash
    # Register celeb user
    curl -X POST https://localhost:5001/api/identity/register?useCookies=true \
      -H "Content-Type: application/json" \
      -d '{"email":"celeb@test.com","password":"Test123!"}'  \
      -c cookies.txt
    
    # Login celeb
    curl -X POST https://localhost:5001/api/identity/login?useCookies=true \
      -H "Content-Type: application/json" \
      -d '{"email":"celeb@test.com","password":"Test123!"}'  \
      -b cookies.txt -c cookies.txt
    
    # Try to follow with exact URL from Postman
    curl -X POST https://localhost:5001/api/profiles/celeb_celeb@test.com/follow \
      -b cookies.txt
    ```

- **What would make bug worse if hypothesis correct?**
  - Analysis: If username format is wrong, using even more complex email format would fail harder

- **What simpler model explains all behavior?**
  - Analysis: **SIMPLEST EXPLANATION**: The `celeb_` prefix in URLs is wrong. Database stores usernames as plain emails (user@mail.com), but Postman uses celeb_user@mail.com.

---

## 18. Resolution & Prevention

- **What specific changes will resolve issues?**
  
  **Priority 1 - Fix Profile URL format (likely fixes 26 failures)**:
  - Remove `celeb_` prefix from all profile URLs in FeedAndArticles and Profiles folders
  - Change `celeb_{{EMAIL}}` → `{{EMAIL}}`
  - This should fix:
    - 12 Follow/Unfollow Profile failures (404 → 200)
    - 14 Feed/Articles failures (cascading from Follow working)
  
  **Priority 2 - Investigate Update Article response format (fixes 8 failures)**:
  - Check if Update Article endpoint has different response model for cookie auth
  - May need to update endpoint to return consistent format
  - Or update test assertions to match actual response
  
  **Priority 3 - Fix authorization status codes (fixes 4 failures)**:
  - Tests expect 403 Forbidden but get 204/200/404
  - Likely need to update test assertions to match actual cookie auth behavior
  
  **Priority 4 - Remaining issues (fixes 11 failures)**:
  - Comment operations
  - Miscellaneous edge cases

- **What test prevents regression?**
  - Analysis: The Postman test suite itself, once at 100%

- **What monitoring would detect earlier?**
  - Analysis: API response format validation in CI

---

## Recommended Next Steps (In Order)

### Step 1: Fix Profile URL Format (Expected Impact: +26 tests, 89% → 95%)
Remove `celeb_` prefix from all Follow/Unfollow URLs:
- In FeedAndArticles folder: Change `/profiles/celeb_{{FEED_USER_EMAIL}}/follow` → `/profiles/{{FEED_USER_EMAIL}}/follow`
- In Profiles folder: Change `/profiles/celeb_{{EMAIL}}/follow` → `/profiles/{{EMAIL}}/follow`

### Step 2: Test Update Article Manually (Expected Impact: Identifies root cause)
Run manual API test to see actual response format with cookie auth

### Step 3: Fix Update Article (Expected Impact: +8 tests, 95% → 97%)
Based on Step 2 findings, either fix API endpoint or update test assertions

### Step 4: Fix Remaining 15 Tests (Expected Impact: +15 tests, 97% → 100%)
Address authorization status codes and comment operation issues

---

## Confidence Assessment

- **High Confidence (80%+)**: Profile URL format is wrong (`celeb_` prefix shouldn't be there)
- **Medium Confidence (50-80%)**: Update Article has response format issue specific to cookie auth
- **Low Confidence (<50%)**: Authorization status codes are API bugs vs test assertion issues
