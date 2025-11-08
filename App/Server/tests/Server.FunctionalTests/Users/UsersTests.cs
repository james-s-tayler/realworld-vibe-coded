using System.Net.Http.Headers;
using Server.Web.Users.GetCurrent;
using Server.Web.Users.Login;
using Server.Web.Users.Register;
using Server.Web.Users.Update;

namespace Server.FunctionalTests.Users;

[Collection("Users Integration Tests")]
public class UsersTests(UsersFixture app) : TestBase<UsersFixture>
{
  [Fact]
  public async Task Register_WithValidCredentials_ReturnsJwtAndUser()
  {
    var request = new RegisterRequest
    {
      User = new UserData
      {
        Email = $"test-{Guid.NewGuid()}@example.com",
        Username = $"testuser-{Guid.NewGuid()}",
        Password = "password123",
      },
    };

    var (response, result) = await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    result.User.ShouldNotBeNull();
    result.User.Token.ShouldNotBeNullOrEmpty();
    result.User.Email.ShouldBe(request.User.Email);
    result.User.Username.ShouldBe(request.User.Username);
  }

  [Fact]
  public async Task Register_WithDuplicateEmail_ReturnsErrorDetail()
  {
    var email = $"duplicate-{Guid.NewGuid()}@example.com";
    var request1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = $"user1-{Guid.NewGuid()}",
        Password = "password123",
      },
    };

    await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request1);

    var request2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = $"user2-{Guid.NewGuid()}",
        Password = "password123",
      },
    };

    var (response, _) = await app.Client.POSTAsync<Register, RegisterRequest, object>(request2);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Login_WithValidCredentials_ReturnsJwtAndUser()
  {
    var email = $"login-{Guid.NewGuid()}@example.com";
    var username = $"loginuser-{Guid.NewGuid()}";
    var password = "password123";

    var registerRequest = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = username,
        Password = password,
      },
    };

    await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);

    var loginRequest = new LoginRequest
    {
      User = new LoginUserData
      {
        Email = email,
        Password = password,
      },
    };

    var (response, result) = await app.Client.POSTAsync<Login, LoginRequest, LoginResponse>(loginRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.ShouldNotBeNull();
    result.User.Token.ShouldNotBeNullOrEmpty();
    result.User.Email.ShouldBe(email);
    result.User.Username.ShouldBe(username);
  }

  [Fact]
  public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
  {
    var loginRequest = new LoginRequest
    {
      User = new LoginUserData
      {
        Email = "nonexistent@example.com",
        Password = "wrongpassword",
      },
    };

    var (response, _) = await app.Client.POSTAsync<Login, LoginRequest, object>(loginRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetCurrentUser_WithValidToken_ReturnsUser()
  {
    var email = $"current-{Guid.NewGuid()}@example.com";
    var username = $"currentuser-{Guid.NewGuid()}";
    var password = "password123";

    var registerRequest = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = username,
        Password = password,
      },
    };

    var (_, registerResult) = await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);

    var token = registerResult.User.Token;

    var client = app.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    });

    var (response, result) = await client.GETAsync<GetCurrent, UserCurrentResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.ShouldNotBeNull();
    result.User.Email.ShouldBe(email);
    result.User.Username.ShouldBe(username);
  }

  [Fact]
  public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
  {
    var (response, _) = await app.Client.GETAsync<GetCurrent, object>();

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Register_WithMissingRequiredFields_ReturnsErrorDetail()
  {
    var request = new RegisterRequest
    {
      User = new UserData
      {
        Email = null!,
        Username = null!,
        Password = null!,
      },
    };

    var (response, _) = await app.Client.POSTAsync<Register, RegisterRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_WithBlankFields_ReturnsErrorDetail()
  {
    var request = new RegisterRequest
    {
      User = new UserData
      {
        Email = string.Empty,
        Username = string.Empty,
        Password = string.Empty,
      },
    };

    var (response, _) = await app.Client.POSTAsync<Register, RegisterRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_WithInvalidEmail_ReturnsErrorDetail()
  {
    var request = new RegisterRequest
    {
      User = new UserData
      {
        Email = "not-an-email",
        Username = $"user-{Guid.NewGuid()}",
        Password = "password123",
      },
    };

    var (response, _) = await app.Client.POSTAsync<Register, RegisterRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_WithDuplicateUsername_ReturnsErrorDetail()
  {
    var username = $"duplicate-username-{Guid.NewGuid()}";
    var request1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = $"user1-{Guid.NewGuid()}@example.com",
        Username = username,
        Password = "password123",
      },
    };

    await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request1);

    var request2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = $"user2-{Guid.NewGuid()}@example.com",
        Username = username,
        Password = "password123",
      },
    };

    var (response, _) = await app.Client.POSTAsync<Register, RegisterRequest, object>(request2);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Login_WithIncorrectPassword_ReturnsUnauthorized()
  {
    var email = $"test-{Guid.NewGuid()}@example.com";
    var username = $"testuser-{Guid.NewGuid()}";
    var password = "correctpassword";

    var registerRequest = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = username,
        Password = password,
      },
    };

    await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);

    var loginRequest = new LoginRequest
    {
      User = new LoginUserData
      {
        Email = email,
        Password = "wrongpassword",
      },
    };

    var (response, _) = await app.Client.POSTAsync<Login, LoginRequest, object>(loginRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
  {
    var loginRequest = new LoginRequest
    {
      User = new LoginUserData
      {
        Email = "nonexistent@example.com",
        Password = "password123",
      },
    };

    var (response, _) = await app.Client.POSTAsync<Login, LoginRequest, object>(loginRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetCurrentUser_WithInvalidToken_ReturnsUnauthorized()
  {
    var client = app.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", "invalid-token");
    });

    var (response, _) = await client.GETAsync<GetCurrent, object>();

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UpdateUser_WithValidData_ReturnsUpdatedUser()
  {
    var email = $"update-{Guid.NewGuid()}@example.com";
    var username = $"updateuser-{Guid.NewGuid()}";
    var password = "password123";

    var registerRequest = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = username,
        Password = password,
      },
    };

    var (_, registerResult) = await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);
    var token = registerResult.User.Token;

    var client = app.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    });

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

    var request1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = existingEmail,
        Username = $"user1-{Guid.NewGuid()}",
        Password = "password123",
      },
    };

    await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request1);

    var request2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = $"user2-{Guid.NewGuid()}@example.com",
        Username = $"user2-{Guid.NewGuid()}",
        Password = "password123",
      },
    };

    var (_, registerResult) = await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request2);
    var token = registerResult.User.Token;

    var client = app.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    });

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

    var request1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = $"user1-{Guid.NewGuid()}@example.com",
        Username = existingUsername,
        Password = "password123",
      },
    };

    await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request1);

    var request2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = $"user2-{Guid.NewGuid()}@example.com",
        Username = $"user2-{Guid.NewGuid()}",
        Password = "password123",
      },
    };

    var (_, registerResult) = await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request2);
    var token = registerResult.User.Token;

    var client = app.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    });

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
    var username = $"testuser-{Guid.NewGuid()}";
    var password = "password123";

    var registerRequest = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = username,
        Password = password,
      },
    };

    var (_, registerResult) = await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);
    var token = registerResult.User.Token;

    var client = app.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    });

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
    var username = $"passworduser-{Guid.NewGuid()}";
    var oldPassword = "oldpassword123";
    var newPassword = "newpassword456";

    var registerRequest = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = username,
        Password = oldPassword,
      },
    };

    var (_, registerResult) = await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);
    var token = registerResult.User.Token;

    var client = app.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    });

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
    var loginRequest = new LoginRequest
    {
      User = new LoginUserData
      {
        Email = email,
        Password = newPassword,
      },
    };

    var (loginResponse, loginResult) = await app.Client.POSTAsync<Login, LoginRequest, LoginResponse>(loginRequest);
    loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    loginResult.User.ShouldNotBeNull();
    loginResult.User.Email.ShouldBe(email);
  }

  [Fact]
  public async Task UpdateUser_WithUsernameChange_UpdatesUsername()
  {
    var email = $"username-test-{Guid.NewGuid()}@example.com";
    var oldUsername = $"olduser-{Guid.NewGuid()}";
    var newUsername = $"newuser-{Guid.NewGuid()}";
    var password = "password123";

    var registerRequest = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = oldUsername,
        Password = password,
      },
    };

    var (_, registerResult) = await app.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);
    var token = registerResult.User.Token;

    var client = app.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    });

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
