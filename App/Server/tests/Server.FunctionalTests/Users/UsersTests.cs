using System.Net.Http.Headers;
using Server.Web.Users.GetCurrent;
using Server.Web.Users.List;
using Server.Web.Users.Update;

namespace Server.FunctionalTests.Users;

public class UsersTests : AppTestBase
{
  public UsersTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
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
    result.User.Roles.ShouldNotBeNull();
    result.User.Roles.ShouldContain("ADMIN");
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
    var loginRequest = new Server.Web.Identity.Login.LoginRequest
    {
      Email = user.Email,
      Password = newPassword,
    };

    var (loginResponse, loginResult) = await Fixture.Client.POSTAsync<Server.Web.Identity.Login.LoginRequest, Server.Web.Identity.Login.LoginResponse>("/api/identity/login?useCookies=false", loginRequest);

    loginResponse.EnsureSuccessStatusCode();

    var newAccessToken = loginResult.AccessToken ?? throw new InvalidOperationException("Login did not return an access token");
    newAccessToken.ShouldNotBeNullOrEmpty();
  }

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

  [Fact]
  public async Task ListUsers_WithAuthentication_ReturnsAllUsers()
  {
    // Arrange - create multiple users
    var tenant = await Fixture.RegisterTenantWithUsersAsync(3);
    var user = tenant.Users[0];

    // Act
    var (response, result) = await user.Client.GETAsync<ListUsers, UsersResponse>();

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Users.ShouldNotBeNull();
    result.Users.Count.ShouldBeGreaterThanOrEqualTo(3);

    // Verify all created users are in the response
    foreach (var createdUser in tenant.Users)
    {
      result.Users.ShouldContain(u => u.Email == createdUser.Email);
    }

    // Verify roles are included
    result.Users.ForEach(u =>
    {
      u.Roles.ShouldNotBeNull();
      u.Roles.ShouldNotBeEmpty();
    });

    // Verify first user (tenant owner) has ADMIN role
    var ownerDto = result.Users.First(u => u.Email == tenant.Users[0].Email);
    ownerDto.Roles.ShouldContain("ADMIN");
  }

  [Fact]
  public async Task ListUsers_WithoutAuthentication_ReturnsUnauthorized()
  {
    // Act
    var (response, _) = await Fixture.Client.GETAsync<ListUsers, object>();

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }
}
