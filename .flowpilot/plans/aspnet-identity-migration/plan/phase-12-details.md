## phase_12

### Phase Overview

Update all postman collections to ignore the token from /api/users/register response and instead make an explicit call to /api/users/login to get the token. This completes the transition to two-step authentication flow across all tests, preparing for switching to Identity endpoints.

### Prerequisites

- Phase 11 completed: Frontend and E2E tests using two-step authentication flow
- Postman collections still using single-step flow (token from register)
- Backend supports both patterns

### Implementation Steps

1. **Update Auth Postman Collection**
   - Open `Test/Postman/Conduit.Auth.postman_collection.json`
   - Find all register requests that extract token from response
   - Update test scripts to NOT save token from register response
   - Add explicit login request after register to get token:
     ```javascript
     // In post-response script after register
     // Remove: pm.environment.set("token", jsonData.user.token);
     
     // Instead, make explicit login call
     pm.sendRequest({
       url: pm.environment.get("baseUrl") + "/api/users/login",
       method: 'POST',
       header: { 'Content-Type': 'application/json' },
       body: {
         mode: 'raw',
         raw: JSON.stringify({
           user: {
             email: pm.environment.get("email"),
             password: pm.environment.get("password")
           }
         })
       }
     }, function (err, response) {
       var jsonData = response.json();
       pm.environment.set("token", jsonData.user.token);
     });
     ```

2. **Test Auth Collection**
   - Run `FOLDER=Auth ./build.sh TestServerPostman`
   - Verify all tests pass with two-step flow
   - Fix any timing or ordering issues

3. **Update Profiles Postman Collection**
   - Open `Test/Postman/Conduit.Profiles.postman_collection.json`
   - Update all register requests to use two-step authentication
   - Remove token extraction from register responses
   - Add explicit login calls after register

4. **Test Profiles Collection**
   - Run `FOLDER=Profiles ./build.sh TestServerPostman`
   - Verify all tests pass

5. **Update FeedAndArticles Postman Collection**
   - Open `Test/Postman/Conduit.FeedAndArticles.postman_collection.json`
   - Update all register requests to use two-step authentication
   - Remove token extraction from register responses
   - Add explicit login calls after register

6. **Test FeedAndArticles Collection**
   - Run `FOLDER=FeedAndArticles ./build.sh TestServerPostman`
   - Verify all tests pass

7. **Update Article Postman Collection**
   - Open `Test/Postman/Conduit.Article.postman_collection.json`
   - Update all register requests to use two-step authentication
   - Remove token extraction from register responses
   - Add explicit login calls after register

8. **Test Article Collection**
   - Run `FOLDER=Article ./build.sh TestServerPostman`
   - Verify all tests pass

9. **Update ArticlesEmpty Postman Collection**
   - Open `Test/Postman/Conduit.ArticlesEmpty.postman_collection.json`
   - Update all register requests to use two-step authentication (if any)
   - Remove token extraction from register responses (if any)
   - Add explicit login calls after register (if any)

10. **Test ArticlesEmpty Collection**
    - Run `FOLDER=ArticlesEmpty ./build.sh TestServerPostman`
    - Verify all tests pass

11. **Verify All Collections Together**
    - Run `./build.sh TestServerPostman`
    - Verify all collections pass in sequence
    - All tests now use two-step authentication flow

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
FOLDER=Auth ./build.sh TestServerPostman
FOLDER=Profiles ./build.sh TestServerPostman
FOLDER=FeedAndArticles ./build.sh TestServerPostman
FOLDER=Article ./build.sh TestServerPostman
FOLDER=ArticlesEmpty ./build.sh TestServerPostman
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass. All tests (frontend, E2E, postman) now use two-step authentication: register then login. This matches Identity's pattern where register doesn't return a token.
