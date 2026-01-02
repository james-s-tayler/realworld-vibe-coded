using System.Net.Http.Headers;
using Server.Web.Users.GetCurrent;
using Server.Web.Users.Update;

namespace Server.FunctionalTests.Users;

[Collection("Users Integration Tests")]
public class UsersTests : AppTestBase<UsersFixture>
{
  public UsersTests(UsersFixture fixture) : base(fixture)
  {
  }

  [Fact]
  public async Task GetCurrentUser_WithValidToken_ReturnsUser()
  {
    var email = $"current-{Guid.NewGuid()}@example.com";
    var password = "password123";

    var (client, _, _) = await Fixture.RegisterTenantAndCreateClientAsync(email, password, TestContext.Current.CancellationToken);

    var (response, result) = await client.GETAsync<GetCurrent, UserCurrentResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.ShouldNotBeNull();
    result.User.Email.ShouldBe(email);
    result.User.Username.ShouldBe(email);
  }

  [Fact]
  public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
  {
    var (response, _) = await Fixture.Client.GETAsync<GetCurrent, object>();

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetCurrentUser_WithInvalidToken_ReturnsUnauthorized()
  {
    var client = Fixture.CreateClient(c =>
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

    var (client, _, _) = await Fixture.RegisterTenantAndCreateClientAsync(email, password, TestContext.Current.CancellationToken);

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

    var (response, _) = await Fixture.Client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UpdateUser_WithDuplicateEmail_ReturnsErrorDetail()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = tenant.Users[0].Email,
      },
    };

    // Act
    var (response, _) = await tenant.Users[1].Client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateUser_WithDuplicateUsername_ReturnsErrorDetail()
  {
    // Arrange
    var existingUsername = $"existing-{Guid.NewGuid()}";
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);

    var updateRequest1 = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = existingUsername,
      },
    };

    await tenant.Users[0].Client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest1);

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = existingUsername,
      },
    };

    // Act
    var (response, _) = await tenant.Users[1].Client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateUser_WithBlankFields_ReturnsErrorDetail()
  {
    var email = $"test-{Guid.NewGuid()}@example.com";
    var password = "password123";

    var (client, _, _) = await Fixture.RegisterTenantAndCreateClientAsync(email, password, TestContext.Current.CancellationToken);

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

    var (client, _, _) = await Fixture.RegisterTenantAndCreateClientAsync(email, oldPassword, TestContext.Current.CancellationToken);

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
    var newAccessToken = await Fixture.LoginUserAsync(email, newPassword, TestContext.Current.CancellationToken);
    newAccessToken.ShouldNotBeNullOrEmpty();
  }

  [Fact]
  public async Task UpdateUser_WithUsernameChange_UpdatesUsername()
  {
    var email = $"username-test-{Guid.NewGuid()}@example.com";
    var newUsername = $"newuser-{Guid.NewGuid()}";
    var password = "password123";

    var (client, _, _) = await Fixture.RegisterTenantAndCreateClientAsync(email, password, TestContext.Current.CancellationToken);

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
