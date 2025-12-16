## phase_6

### Phase Overview

Make the username parameter optional on the /api/users/register endpoint, defaulting it to the email value when not provided or null. Update the Auth postman collection to remove username from test data. This establishes a migration pathway for incrementally updating other collections, frontend, and E2E tests.

### Prerequisites

- Phase 5 completed: Postman collections split into separate files
- All collections passing independently
- Backend still using JWT authentication via /api/users endpoints

### Implementation Steps

1. **Update RegisterUserCommand**
   - Open `App/Server/src/Server.UseCases/Users/Register/RegisterUserCommand.cs`
   - Consider making Username optional or provide a default value strategy
   - Document the behavior: when Username is null/empty, use Email as Username

2. **Update UserData DTO**
   - Open `App/Server/src/Server.Web/Users/Register/UserData.cs`
   - Make Username property nullable: `public string? Username { get; set; }`
   - This allows clients to omit the username field

3. **Update RegisterValidator**
   - Open `App/Server/src/Server.Web/Users/Register/RegisterValidator.cs`
   - Update Username validation rules to be conditional (only validate if provided)
   - Or remove username validation entirely if always defaulting to email

4. **Update Register Endpoint Mapping**
   - Open `App/Server/src/Server.Web/Users/Register/Register.cs`
   - Update the mapping from UserData to RegisterUserCommand:
     ```csharp
     var command = new RegisterUserCommand(
         req.User.Email,
         req.User.Username ?? req.User.Email,  // Default to email if username not provided
         req.User.Password
     );
     ```

5. **Update RegisterUserHandler (if needed)**
   - Open `App/Server/src/Server.UseCases/Users/Register/RegisterUserHandler.cs`
   - Verify the handler correctly uses the username value (which may be email)
   - No changes should be needed if the command is mapped correctly

6. **Test Backend Changes**
   - Run `./build.sh BuildServer` to ensure compilation succeeds
   - Run `./build.sh TestServer` to ensure functional tests pass
   - Tests may need updating if they expect username to be different from email

7. **Update Auth Postman Collection**
   - Open `Test/Postman/Conduit.Auth.postman_collection.json`
   - Find all register requests
   - Remove `username` field from request bodies:
     ```json
     {
       "user": {
         "email": "test@example.com",
         "password": "password123"
       }
     }
     ```
   - Remove username from test assertions if any

8. **Update Auth Collection Test Variables**
   - Review any collection variables or environment variables related to username
   - Update test setup to not include username
   - Ensure tests expect username to equal email

9. **Test Auth Collection**
   - Run `./build.sh TestServerPostmanAuth`
   - Verify all Auth tests pass without providing username
   - Verify users are created with email as username

10. **Verify Other Collections Still Pass**
    - Run `./build.sh TestServerPostmanProfiles`
    - Run `./build.sh TestServerPostmanFeedAndArticles`
    - Run `./build.sh TestServerPostmanArticle`
    - Verify they still pass (they may still be providing username, which is fine)

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

All targets must pass. Auth collection should not provide username and should use email as username. Other collections may still provide username explicitly and should continue to work.