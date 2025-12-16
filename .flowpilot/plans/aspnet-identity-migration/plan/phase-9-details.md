## phase_9

### Phase Overview

Update the Article and ArticlesEmpty postman collections to remove username dependency. When registering test users, do not provide username field - it will default to email. Update all test setup data to remove username references. This completes the username removal from all postman collections.

### Prerequisites

- Phase 8 completed: FeedAndArticles collection updated and passing without username
- Auth, Profiles, and FeedAndArticles collections using email as username
- Article and ArticlesEmpty collections still passing with or without username

### Implementation Steps

1. **Open Article Postman Collection**
   - Open `Test/Postman/Conduit.Article.postman_collection.json`
   - Review all requests that register users for test setup
   - Identify any variables or test data that reference username

2. **Update Registration Requests in Article Collection**
   - Find all POST /api/users/register requests
   - Remove `username` field from request bodies:
     ```json
     {
       "user": {
         "email": "testuser@example.com",
         "password": "password123"
       }
     }
     ```
   - Ensure only email and password are provided

3. **Update Article Collection Variables and Scripts**
   - Review collection-level variables
   - Remove or update any username-related variables
   - Update pre-request scripts to use email instead of separate username
   - Update test assertions to expect username to equal email

4. **Test Article Collection Independently**
   - Run `FOLDER=Article ./build.sh TestServerPostman`
   - Verify all tests pass
   - Verify users are registered with email as username

5. **Open ArticlesEmpty Postman Collection**
   - Open `Test/Postman/Conduit.ArticlesEmpty.postman_collection.json`
   - Review all requests that register users for test setup
   - Identify any variables or test data that reference username

6. **Update Registration Requests in ArticlesEmpty Collection**
   - Find all POST /api/users/register requests (if any)
   - Remove `username` field from request bodies
   - Ensure only email and password are provided

7. **Update ArticlesEmpty Collection Variables and Scripts**
   - Review collection-level variables
   - Remove or update any username-related variables
   - Update pre-request scripts if needed
   - Update test assertions if needed

8. **Test ArticlesEmpty Collection Independently**
   - Run `FOLDER=ArticlesEmpty ./build.sh TestServerPostman`
   - Verify all tests pass

9. **Verify All Collections Pass Together**
   - Run `./build.sh TestServerPostman`
   - Verify all collections pass in sequence
   - All postman collections now use email as username

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

All targets must pass. All postman collections should now use email as username. Frontend and E2E tests still use explicit username but that will be addressed in the next phase.
