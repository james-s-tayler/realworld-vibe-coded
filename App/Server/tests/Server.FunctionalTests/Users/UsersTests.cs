using Server.Web.Users.GetCurrent;
using Server.Web.Users.Update;

namespace Server.FunctionalTests.Users;

[Collection("Users Integration Tests")]
public class UsersTests(UsersFixture app) : TestBase<UsersFixture>
{
  [Fact]
  public async Task GetCurrentUser_WithValidCookie_ReturnsUser()
  {
    var email = $"current-{Guid.NewGuid()}@example.com";
    var username = $"currentuser-{Guid.NewGuid()}";
    var password = "password123!";

    // SRV007: CreateAuthenticatedClient uses PostAsJsonAsync internally to register via Identity API
#pragma warning disable SRV007
    var client = await CreateAuthenticatedClient(email, username, password);
#pragma warning restore SRV007

    var (response, result) = await client.GETAsync<GetCurrent, UserCurrentResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.ShouldNotBeNull();
    result.User.Email.ShouldBe(email);
    result.User.Username.ShouldBe(username);
  }

  [Fact]
  public async Task GetCurrentUser_WithoutCookie_ReturnsUnauthorized()
  {
    var (response, _) = await app.Client.GETAsync<GetCurrent, object>();

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UpdateUser_WithValidData_ReturnsUpdatedUser()
  {
    var email = $"update-{Guid.NewGuid()}@example.com";
    var username = $"updateuser-{Guid.NewGuid()}";
    var password = "password123!";

    // SRV007: CreateAuthenticatedClient uses PostAsJsonAsync internally to register via Identity API
#pragma warning disable SRV007
    var client = await CreateAuthenticatedClient(email, username, password);
#pragma warning restore SRV007

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = $"updated-{Guid.NewGuid()}@example.com",
        Bio = "Updated bio",
        Image = "https://example.com/image.jpg",
      },
    };

    var (response, result) = await client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.ShouldNotBeNull();
    result.User.Email.ShouldBe(updateRequest.User.Email);
    result.User.Bio.ShouldBe(updateRequest.User.Bio);
    result.User.Image.ShouldBe(updateRequest.User.Image);
  }

  [Fact]
  public async Task UpdateUser_WithoutAuthentication_ReturnsUnauthorized()
  {
    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = "test@example.com",
      },
    };

    var (response, _) = await app.Client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UpdateUser_WithDuplicateEmail_ReturnsErrorDetail()
  {
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user1Username = $"user1-{Guid.NewGuid()}";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";
    var user2Username = $"user2-{Guid.NewGuid()}";
    var password = "password123!";

    // Create first user
    // SRV007: CreateAuthenticatedClient uses PostAsJsonAsync internally to register via Identity API
#pragma warning disable SRV007
    await CreateAuthenticatedClient(user1Email, user1Username, password);
#pragma warning restore SRV007

    // Create second user
#pragma warning disable SRV007
    var client2 = await CreateAuthenticatedClient(user2Email, user2Username, password);
#pragma warning restore SRV007

    // Try to update second user's email to first user's email
    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = user1Email,
      },
    };

    var (response, _) = await client2.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
  }

  [Fact]
  public async Task UpdateUser_WithDuplicateUsername_ReturnsErrorDetail()
  {
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user1Username = $"user1-{Guid.NewGuid()}";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";
    var user2Username = $"user2-{Guid.NewGuid()}";
    var password = "password123!";

    // Create first user
#pragma warning disable SRV007
    await CreateAuthenticatedClient(user1Email, user1Username, password);
#pragma warning restore SRV007

    // Create second user
#pragma warning disable SRV007
    var client2 = await CreateAuthenticatedClient(user2Email, user2Username, password);
#pragma warning restore SRV007

    // Try to update second user's username to first user's username
    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = user1Username,
      },
    };

    var (response, _) = await client2.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
  }

  [Fact]
  public async Task UpdateUser_WithUsernameChange_UpdatesUsername()
  {
    var email = $"update-{Guid.NewGuid()}@example.com";
    var username = $"updateuser-{Guid.NewGuid()}";
    var newUsername = $"newusername-{Guid.NewGuid()}";
    var password = "password123!";

    // SRV007: CreateAuthenticatedClient uses PostAsJsonAsync internally to register via Identity API
#pragma warning disable SRV007
    var client = await CreateAuthenticatedClient(email, username, password);
#pragma warning restore SRV007

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = newUsername,
      },
    };

    var (response, result) = await client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.ShouldNotBeNull();
    result.User.Username.ShouldBe(newUsername);
  }

  private async Task<HttpClient> CreateAuthenticatedClient(string email, string username, string password)
  {
    // Register user via Identity endpoint
    // SRV007: Using raw HttpClient.PostAsJsonAsync is necessary here to register via Identity API
    // which doesn't have a FastEndpoints endpoint type we can reference.
#pragma warning disable SRV007
    var registerRequest = new
    {
      email,
      username,
      password,
    };

    var registerResponse = await app.Client.PostAsJsonAsync("/api/identity/register", registerRequest);
#pragma warning restore SRV007
    registerResponse.EnsureSuccessStatusCode();

    // Login to get cookies
#pragma warning disable SRV007
    var loginRequest = new
    {
      email,
      password,
    };

    var loginResponse = await app.Client.PostAsJsonAsync("/api/identity/login", loginRequest);
#pragma warning restore SRV007
    loginResponse.EnsureSuccessStatusCode();

    // Extract cookies from login response
    if (!loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
    {
      throw new Exception("No cookies returned from login endpoint");
    }

    // Create new client with cookies
    var client = app.CreateClient(c =>
    {
      foreach (var cookie in cookies)
      {
        c.DefaultRequestHeaders.Add("Cookie", cookie.Split(';')[0]);
      }
    });

    return client;
  }
}
