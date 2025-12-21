using System.Net.Http.Headers;
using Server.Web.Users.GetCurrent;
using Server.Web.Users.Update;

namespace Server.FunctionalTests.Users;

[Collection("Users Integration Tests")]
public class UsersTests(UsersFixture app) : TestBase<UsersFixture>
{
  [Fact]
  public async Task GetCurrentUser_WithValidToken_ReturnsUser()
  {
    var email = $"current-{Guid.NewGuid()}@example.com";
    var password = "password123";

    var (client, _, accessToken) = await app.RegisterUserAndCreateClientAsync(email, password, TestContext.Current.CancellationToken);

    var (response, result) = await client.GETAsync<GetCurrent, UserCurrentResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.ShouldNotBeNull();
    result.User.Email.ShouldBe(email);
    result.User.Username.ShouldBe(email); // Username defaults to email
  }

  [Fact]
  public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
  {
    var (response, _) = await app.Client.GETAsync<GetCurrent, object>();

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetCurrentUser_WithInvalidToken_ReturnsUnauthorized()
  {
    var client = app.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");
    });

    var (response, _) = await client.GETAsync<GetCurrent, object>();

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UpdateUser_WithValidData_ReturnsUpdatedUser()
  {
    var email = $"update-{Guid.NewGuid()}@example.com";
    var password = "password123";

    var (client, _, _) = await app.RegisterUserAndCreateClientAsync(email, password, TestContext.Current.CancellationToken);

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
    var existingEmail = $"existing-{Guid.NewGuid()}@example.com";

    // Register first user with existing email
    await app.RegisterUserAsync(existingEmail, "password123", TestContext.Current.CancellationToken);

    // Register second user
    var email2 = $"user2-{Guid.NewGuid()}@example.com";
    var (client, _, _) = await app.RegisterUserAndCreateClientAsync(email2, "password123", TestContext.Current.CancellationToken);

    // Try to update second user's email to the existing email
    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = existingEmail,
      },
    };

    var (response, _) = await client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateUser_WithDuplicateUsername_ReturnsErrorDetail()
  {
    var existingUsername = $"existing-{Guid.NewGuid()}";

    // Register first user
    var email1 = $"user1-{Guid.NewGuid()}@example.com";
    await app.RegisterUserAsync(email1, "password123", TestContext.Current.CancellationToken);

    // Register second user
    var email2 = $"user2-{Guid.NewGuid()}@example.com";
    var (client, _, _) = await app.RegisterUserAndCreateClientAsync(email2, "password123", TestContext.Current.CancellationToken);

    // Try to update second user's username to the existing one
    // Note: Since Identity defaults username to email, we need to first update user1's username
    var accessToken1 = await app.LoginUserAsync(email1, "password123", TestContext.Current.CancellationToken);
    var updateRequest1 = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = existingUsername,
      },
    };

    // SRV007: CreateAuthenticatedClient is a test fixture helper method that creates an HttpClient with auth headers.
    // This is not a raw HTTP call but a necessary test setup step.
#pragma warning disable SRV007
    var client1Auth = app.CreateAuthenticatedClient(accessToken1);
#pragma warning restore SRV007
    await client1Auth.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest1);

    // Now try to set user2's username to the same
    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = existingUsername,
      },
    };

    var (response, _) = await client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateUser_WithBlankFields_ReturnsErrorDetail()
  {
    var email = $"test-{Guid.NewGuid()}@example.com";
    var password = "password123";

    var (client, _, _) = await app.RegisterUserAndCreateClientAsync(email, password, TestContext.Current.CancellationToken);

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = string.Empty,
      },
    };

    var (response, _) = await client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateUser_WithNewPassword_CanLoginWithNewPassword()
  {
    var email = $"password-test-{Guid.NewGuid()}@example.com";
    var oldPassword = "oldpassword123";
    var newPassword = "newpassword456";

    var (client, _, _) = await app.RegisterUserAndCreateClientAsync(email, oldPassword, TestContext.Current.CancellationToken);

    // Update password
    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Password = newPassword,
      },
    };

    var (response, result) = await client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest);
    response.StatusCode.ShouldBe(HttpStatusCode.OK);

    // Try to login with new password
    var newAccessToken = await app.LoginUserAsync(email, newPassword, TestContext.Current.CancellationToken);
    newAccessToken.ShouldNotBeNullOrEmpty();
  }

  [Fact]
  public async Task UpdateUser_WithUsernameChange_UpdatesUsername()
  {
    var email = $"username-test-{Guid.NewGuid()}@example.com";
    var newUsername = $"newuser-{Guid.NewGuid()}";
    var password = "password123";

    var (client, _, _) = await app.RegisterUserAndCreateClientAsync(email, password, TestContext.Current.CancellationToken);

    // Update username
    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = newUsername,
      },
    };

    var (response, result) = await client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest);
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.Username.ShouldBe(newUsername);
  }
}
