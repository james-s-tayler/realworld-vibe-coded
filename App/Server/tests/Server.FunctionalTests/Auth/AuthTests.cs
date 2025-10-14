using System.Net;
using System.Net.Http.Headers;
using Server.Web.Users;

namespace Server.FunctionalTests.Auth;

[Collection("Auth Integration Tests")]
public class AuthTests(AuthFixture App) : TestBase<AuthFixture>
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

    var (response, result) = await App.Client.POSTAsync<Server.Web.Users.Register, RegisterRequest, RegisterResponse>(request);

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

    await App.Client.POSTAsync<Server.Web.Users.Register, RegisterRequest, RegisterResponse>(request1);

    var request2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = $"user2-{Guid.NewGuid()}",
        Password = "password123"
      }
    };

    var (response, _) = await App.Client.POSTAsync<Server.Web.Users.Register, RegisterRequest, object>(request2);

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

    await App.Client.POSTAsync<Server.Web.Users.Register, RegisterRequest, RegisterResponse>(registerRequest);

    var loginRequest = new LoginRequest
    {
      User = new LoginUserData
      {
        Email = email,
        Password = password
      }
    };

    var (response, result) = await App.Client.POSTAsync<Server.Web.Users.Login, LoginRequest, LoginResponse>(loginRequest);

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

    var (response, _) = await App.Client.POSTAsync<Server.Web.Users.Login, LoginRequest, object>(loginRequest);

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

    var (_, registerResult) = await App.Client.POSTAsync<Server.Web.Users.Register, RegisterRequest, RegisterResponse>(registerRequest);

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
}
