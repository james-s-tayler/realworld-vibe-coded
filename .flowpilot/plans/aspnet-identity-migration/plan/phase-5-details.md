## phase_5

### Phase Overview

Split the monolithic Postman collection into individual collections for better maintainability and isolated testing. This allows each area (Auth, Profiles, Feed, Articles) to be tested independently and makes debugging easier.

### Prerequisites

- Phase 4 completed: Backend fully migrated to ApplicationUser
- All tests passing with JWT authentication via /api/users endpoints
- Dual authentication (JWT and Identity) operational

### Implementation Steps

1. **Analyze Monolithic Collection Structure**
   - Open the existing monolithic Postman collection
   - Identify logical groupings: Auth, Profiles, Feed/Articles, Articles
   - Note test dependencies and setup requirements for each group
   - Document the order in which tests must run

2. **Create Separate Collection Files**
   - Create `Conduit.Auth.postman_collection.json` - authentication tests only
   - Create `Conduit.Profiles.postman_collection.json` - profile-related tests
   - Create `Conduit.FeedAndArticles.postman_collection.json` - feed and article list tests
   - Create `Conduit.Article.postman_collection.json` - individual article operations
   - Create `Conduit.ArticlesEmpty.postman_collection.json` - article tests with empty database

3. **Split Test Cases by Domain**
   - Move all register/login tests to Auth collection
   - Move profile viewing/following tests to Profiles collection
   - Move feed and article listing tests to FeedAndArticles collection
   - Move article CRUD and comment tests to Article collection
   - Move empty state tests to ArticlesEmpty collection

4. **Replicate Test Setup in Each Collection**
   - Each collection needs its own user registration/login for test setup
   - Ensure pre-request scripts handle authentication appropriately
   - Copy any shared variables or environment setup to each collection
   - Ensure each collection can run independently

5. **Update Nuke Build Targets**
   - Add individual Nuke targets for each collection:
     - `TestServerPostmanAuth`
     - `TestServerPostmanProfiles`
     - `TestServerPostmanFeed`
     - `TestServerPostmanArticle`
     - `TestServerPostmanArticlesEmpty`
   - Update `TestServerPostman` to run all collections
   - Add support for `FOLDER` environment variable to run specific collection

6. **Create Docker Compose Files for Each Collection**
   - Create `docker-compose.Auth.yml` for running Auth tests in docker
   - Create similar files for other collections
   - Ensure each compose file references correct collection file
   - Test each collection can run in isolation via docker

7. **Test Each Collection Independently**
   - Run `./build.sh TestServerPostmanAuth` to test Auth collection
   - Run each other collection independently
   - Verify all collections pass
   - Verify `./build.sh TestServerPostman` runs all collections

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

All targets must pass. Each collection must be runnable independently and all collections must pass when run together.