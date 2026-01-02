using System.Net.Http.Headers;
using Server.Web.Users.GetCurrent;
using Server.Web.Users.Update;

namespace Server.FunctionalTests.Users;

[Collection("Users Integration Tests")]
public class UsersTests : AppTestBase<ApiFixture>
{
  public UsersTests(ApiFixture fixture) : base(fixture)
  {
  }

  [Fact]
  public async Task GetCurrentUser_WithValidToken_ReturnsUser()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var (response, result) = await user.Client.GETAsync<GetCurrent, UserCurrentResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.ShouldNotBeNull();
    result.User.Email.ShouldBe(user.Email);
    result.User.Username.ShouldBe(user.Email);
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
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = $"updated-{Guid.NewGuid()}@example.com",
        Bio = "Updated bio",
        Image = "https://example.com/image.jpg",
      },
    };

    var (response, result) = await user.Client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest);

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
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = string.Empty,
      },
    };

    var (response, _) = await user.Client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateUser_WithNewPassword_CanLoginWithNewPassword()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];
    var newPassword = "newpassword456";

    // Update password
    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Password = newPassword,
      },
    };

    var (response, result) = await user.Client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest);
    response.StatusCode.ShouldBe(HttpStatusCode.OK);

    // Try to login with new password
    var loginPayload = new
    {
      email = user.Email,
      password = newPassword,
    };

    var loginResponse = await Fixture.Client.PostAsJsonAsync(
      "/api/identity/login?useCookies=false",
      loginPayload,
      TestContext.Current.CancellationToken);

    loginResponse.EnsureSuccessStatusCode();

    var loginResult = await loginResponse.Content.ReadFromJsonAsync<IdentityLoginResponse>(TestContext.Current.CancellationToken);
    var newAccessToken = loginResult?.AccessToken ?? throw new InvalidOperationException("Login did not return an access token");
    newAccessToken.ShouldNotBeNullOrEmpty();
  }

  private record IdentityLoginResponse(string AccessToken);

  [Fact]
  public async Task UpdateUser_WithUsernameChange_UpdatesUsername()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];
    var newUsername = $"newuser-{Guid.NewGuid()}";

    // Update username
    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = newUsername,
      },
    };

    var (response, result) = await user.Client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest);
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.Username.ShouldBe(newUsername);
  }
}
