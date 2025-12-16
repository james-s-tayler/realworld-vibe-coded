## phase_8

### Phase Overview

Update the FeedAndArticles postman collection to remove username dependency. When registering test users, do not provide username field - it will default to email. Update all test setup data to remove username references.

### Prerequisites

- Phase 7 completed: Profiles collection updated and passing without username
- Auth and Profiles collections using email as username
- Other collections still passing with or without username

### Implementation Steps

1. **Open FeedAndArticles Postman Collection**
   - Open `Test/Postman/Conduit.FeedAndArticles.postman_collection.json`
   - Review all requests that register users for test setup
   - Identify any variables or test data that reference username

2. **Update Registration Requests in FeedAndArticles Collection**
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

3. **Update Collection Variables**
   - Review collection-level variables
   - Remove or update any username-related variables
   - Ensure variables use email values where username was previously used

4. **Update Pre-Request Scripts**
   - Review pre-request scripts that may set username variables
   - Update to use email instead of separate username
   - Ensure dynamic user generation uses email format

5. **Update Test Assertions**
   - Review test scripts that assert username values
   - Update to expect username to equal email
   - Remove any assertions that compare username to a different value than email

6. **Update Test Data in Request Bodies**
   - Review any request bodies that may reference usernames
   - Ensure article and feed operations work with email-based usernames
   - Update any hardcoded test data

7. **Test FeedAndArticles Collection Independently**
   - Run `FOLDER=FeedAndArticles ./build.sh TestServerPostman`
   - Verify all tests pass
   - Verify users are registered with email as username
   - Verify feed and article list operations work correctly

8. **Verify Previously Updated Collections Still Pass**
   - Run `FOLDER=Auth ./build.sh TestServerPostman` - should still pass
   - Run `FOLDER=Profiles ./build.sh TestServerPostman` - should still pass
   - Run `FOLDER=Article ./build.sh TestServerPostman` - not yet updated
   - Run `FOLDER=ArticlesEmpty ./build.sh TestServerPostman` - not yet updated

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

All targets must pass. Auth, Profiles, and FeedAndArticles collections should now use email as username. Article and ArticlesEmpty collections may still provide username explicitly and should continue to work.
