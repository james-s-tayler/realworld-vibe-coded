using System.Net.Http.Headers;
using Server.Web.Users.GetCurrent;
using Server.Web.Users.Login;
using Server.Web.Users.Register;
using Server.Web.Users.Update;

namespace Server.FunctionalTests.Users;

[Collection("Users Integration Tests")]
public class UsersTests(UsersFixture App) : TestBase<UsersFixture>
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
        Password = "password123"
      }
    };

    var (response, result) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    result.User.ShouldNotBeNull();
    result.User.Token.ShouldNotBeNullOrEmpty();
    result.User.Email.ShouldBe(request.User.Email);
    result.User.Username.ShouldBe(request.User.Username);
  }

  [Fact]
  public async Task Register_WithDuplicateEmail_ReturnsValidationError()
  {
    var email = $"duplicate-{Guid.NewGuid()}@example.com";
    var request1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = $"user1-{Guid.NewGuid()}",
        Password = "password123"
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request1);

    var request2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = $"user2-{Guid.NewGuid()}",
        Password = "password123"
      }
    };

    var (response, _) = await App.Client.POSTAsync<Register, RegisterRequest, object>(request2);

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
        Password = password
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);

    var loginRequest = new LoginRequest
    {
      User = new LoginUserData
      {
        Email = email,
        Password = password
      }
    };

    var (response, result) = await App.Client.POSTAsync<Login, LoginRequest, LoginResponse>(loginRequest);

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
        Password = "wrongpassword"
      }
    };

    var (response, _) = await App.Client.POSTAsync<Login, LoginRequest, object>(loginRequest);

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
        Password = password
      }
    };

    var (_, registerResult) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);

    var token = registerResult.User.Token;

    var client = App.CreateClient(c =>
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
    var (response, _) = await App.Client.GETAsync<GetCurrent, object>();

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Register_WithMissingRequiredFields_ReturnsValidationError()
  {
    var request = new RegisterRequest
    {
      User = new UserData
      {
        Email = null!,
        Username = null!,
        Password = null!
      }
    };

    var (response, _) = await App.Client.POSTAsync<Register, RegisterRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_WithBlankFields_ReturnsValidationError()
  {
    var request = new RegisterRequest
    {
      User = new UserData
      {
        Email = "",
        Username = "",
        Password = ""
      }
    };

    var (response, _) = await App.Client.POSTAsync<Register, RegisterRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_WithInvalidEmail_ReturnsValidationError()
  {
    var request = new RegisterRequest
    {
      User = new UserData
      {
        Email = "not-an-email",
        Username = $"user-{Guid.NewGuid()}",
        Password = "password123"
      }
    };

    var (response, _) = await App.Client.POSTAsync<Register, RegisterRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_WithDuplicateUsername_ReturnsValidationError()
  {
    var username = $"duplicate-username-{Guid.NewGuid()}";
    var request1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = $"user1-{Guid.NewGuid()}@example.com",
        Username = username,
        Password = "password123"
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request1);

    var request2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = $"user2-{Guid.NewGuid()}@example.com",
        Username = username,
        Password = "password123"
      }
    };

    var (response, _) = await App.Client.POSTAsync<Register, RegisterRequest, object>(request2);

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
        Password = password
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);

    var loginRequest = new LoginRequest
    {
      User = new LoginUserData
      {
        Email = email,
        Password = "wrongpassword"
      }
    };

    var (response, _) = await App.Client.POSTAsync<Login, LoginRequest, object>(loginRequest);

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
        Password = "password123"
      }
    };

    var (response, _) = await App.Client.POSTAsync<Login, LoginRequest, object>(loginRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetCurrentUser_WithInvalidToken_ReturnsUnauthorized()
  {
    var client = App.CreateClient(c =>
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
        Password = password
      }
    };

    var (_, registerResult) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);
    var token = registerResult.User.Token;

    var client = App.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    });

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = $"updated-{Guid.NewGuid()}@example.com",
        Bio = "Updated bio",
        Image = "https://example.com/image.jpg"
      }
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
        Email = "test@example.com"
      }
    };

    var (response, _) = await App.Client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UpdateUser_WithDuplicateEmail_ReturnsValidationError()
  {
    var existingEmail = $"existing-{Guid.NewGuid()}@example.com";

    var request1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = existingEmail,
        Username = $"user1-{Guid.NewGuid()}",
        Password = "password123"
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request1);

    var request2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = $"user2-{Guid.NewGuid()}@example.com",
        Username = $"user2-{Guid.NewGuid()}",
        Password = "password123"
      }
    };

    var (_, registerResult) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request2);
    var token = registerResult.User.Token;

    var client = App.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    });

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = existingEmail
      }
    };

    var (response, _) = await client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateUser_WithDuplicateUsername_ReturnsValidationError()
  {
    var existingUsername = $"existing-{Guid.NewGuid()}";

    var request1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = $"user1-{Guid.NewGuid()}@example.com",
        Username = existingUsername,
        Password = "password123"
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request1);

    var request2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = $"user2-{Guid.NewGuid()}@example.com",
        Username = $"user2-{Guid.NewGuid()}",
        Password = "password123"
      }
    };

    var (_, registerResult) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(request2);
    var token = registerResult.User.Token;

    var client = App.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    });

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = existingUsername
      }
    };

    var (response, _) = await client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateUser_WithBlankFields_ReturnsValidationError()
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
        Password = password
      }
    };

    var (_, registerResult) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);
    var token = registerResult.User.Token;

    var client = App.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    });

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = ""
      }
    };

    var (response, _) = await client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }
}
