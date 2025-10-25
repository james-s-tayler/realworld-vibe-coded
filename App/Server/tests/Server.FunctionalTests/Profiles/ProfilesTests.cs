using System.Net.Http.Headers;
using Server.Web.Profiles;
using Server.Web.Profiles.Follow;
using Server.Web.Profiles.Get;
using Server.Web.Profiles.Unfollow;
using Server.Web.Users.Register;

namespace Server.FunctionalTests.Profiles;

[Collection("Profiles Integration Tests")]
public class ProfilesTests(ProfilesFixture App) : TestBase<ProfilesFixture>
{
  [Fact]
  public async Task GetProfile_Unauthenticated_ReturnsProfile()
  {
    var username = $"testuser-{Guid.NewGuid()}";
    var email = $"test-{Guid.NewGuid()}@example.com";

    var registerRequest = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = username,
        Password = "password123"
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);

    var request = new GetProfileRequest { Username = username };
    var (response, result) = await App.Client.GETAsync<Get, GetProfileRequest, ProfileResponse>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(username);
    result.Profile.Following.ShouldBeFalse();
  }

  [Fact]
  public async Task GetProfile_Authenticated_NotFollowing_ReturnsProfile()
  {
    var user1Username = $"user1-{Guid.NewGuid()}";
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user2Username = $"user2-{Guid.NewGuid()}";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";

    var register1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = user1Email,
        Username = user1Username,
        Password = "password123"
      }
    };

    var (_, result1) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(register1);
    var user1Token = result1.User.Token;

    var register2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = user2Email,
        Username = user2Username,
        Password = "password123"
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(register2);

    var client = App.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", user1Token);
    });

    var request = new GetProfileRequest { Username = user2Username };
    var (response, result) = await client.GETAsync<Get, GetProfileRequest, ProfileResponse>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(user2Username);
    result.Profile.Following.ShouldBeFalse();
  }

  [Fact]
  public async Task GetProfile_Authenticated_Following_ReturnsProfileWithFollowing()
  {
    var user1Username = $"user1-{Guid.NewGuid()}";
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user2Username = $"user2-{Guid.NewGuid()}";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";

    var register1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = user1Email,
        Username = user1Username,
        Password = "password123"
      }
    };

    var (_, result1) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(register1);
    var user1Token = result1.User.Token;

    var register2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = user2Email,
        Username = user2Username,
        Password = "password123"
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(register2);

    var client = App.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", user1Token);
    });

    var followRequest = new FollowProfileRequest { Username = user2Username };
    await client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    var getRequest = new GetProfileRequest { Username = user2Username };
    var (response, result) = await client.GETAsync<Get, GetProfileRequest, ProfileResponse>(getRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(user2Username);
    result.Profile.Following.ShouldBeTrue();
  }

  [Fact]
  public async Task GetProfile_NonExistentUser_ReturnsNotFound()
  {
    var request = new GetProfileRequest { Username = "nonexistentuser999" };
    var (response, _) = await App.Client.GETAsync<Get, GetProfileRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetProfile_InvalidUsername_ReturnsNotFound()
  {
    var request = new GetProfileRequest { Username = "invalid user!" };
    var (response, _) = await App.Client.GETAsync<Get, GetProfileRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task FollowProfile_WithAuthentication_ReturnsProfileWithFollowing()
  {
    var user1Username = $"user1-{Guid.NewGuid()}";
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user2Username = $"user2-{Guid.NewGuid()}";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";

    var register1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = user1Email,
        Username = user1Username,
        Password = "password123"
      }
    };

    var (_, result1) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(register1);
    var user1Token = result1.User.Token;

    var register2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = user2Email,
        Username = user2Username,
        Password = "password123"
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(register2);

    var client = App.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", user1Token);
    });

    var followRequest = new FollowProfileRequest { Username = user2Username };
    var (response, result) = await client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(user2Username);
    result.Profile.Following.ShouldBeTrue();
  }

  [Fact]
  public async Task FollowProfile_AlreadyFollowing_ReturnsProfileWithFollowing()
  {
    var user1Username = $"user1-{Guid.NewGuid()}";
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user2Username = $"user2-{Guid.NewGuid()}";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";

    var register1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = user1Email,
        Username = user1Username,
        Password = "password123"
      }
    };

    var (_, result1) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(register1);
    var user1Token = result1.User.Token;

    var register2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = user2Email,
        Username = user2Username,
        Password = "password123"
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(register2);

    var client = App.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", user1Token);
    });

    var followRequest = new FollowProfileRequest { Username = user2Username };
    await client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    var (response, result) = await client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Following.ShouldBeTrue();
  }

  [Fact]
  public async Task FollowProfile_WithoutAuthentication_ReturnsUnauthorized()
  {
    var followRequest = new FollowProfileRequest { Username = "someuser" };
    var (response, _) = await App.Client.POSTAsync<Follow, FollowProfileRequest, object>(followRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task FollowProfile_NonExistentUser_ReturnsNotFound()
  {
    var userEmail = $"user-{Guid.NewGuid()}@example.com";
    var username = $"user-{Guid.NewGuid()}";

    var registerRequest = new RegisterRequest
    {
      User = new UserData
      {
        Email = userEmail,
        Username = username,
        Password = "password123"
      }
    };

    var (_, result) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);
    var token = result.User.Token;

    var client = App.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    });

    var followRequest = new FollowProfileRequest { Username = "nonexistentuser999" };
    var (response, _) = await client.POSTAsync<Follow, FollowProfileRequest, object>(followRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UnfollowProfile_WithAuthentication_ReturnsProfileWithoutFollowing()
  {
    var user1Username = $"user1-{Guid.NewGuid()}";
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user2Username = $"user2-{Guid.NewGuid()}";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";

    var register1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = user1Email,
        Username = user1Username,
        Password = "password123"
      }
    };

    var (_, result1) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(register1);
    var user1Token = result1.User.Token;

    var register2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = user2Email,
        Username = user2Username,
        Password = "password123"
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(register2);

    var client = App.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", user1Token);
    });

    var followRequest = new FollowProfileRequest { Username = user2Username };
    await client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    var unfollowRequest = new UnfollowProfileRequest { Username = user2Username };
    var (response, result) = await client.DELETEAsync<Unfollow, UnfollowProfileRequest, ProfileResponse>(unfollowRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(user2Username);
    result.Profile.Following.ShouldBeFalse();
  }

  [Fact]
  public async Task UnfollowProfile_NotFollowing_ReturnsValidationError()
  {
    var user1Username = $"user1-{Guid.NewGuid()}";
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user2Username = $"user2-{Guid.NewGuid()}";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";

    var register1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = user1Email,
        Username = user1Username,
        Password = "password123"
      }
    };

    var (_, result1) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(register1);
    var user1Token = result1.User.Token;

    var register2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = user2Email,
        Username = user2Username,
        Password = "password123"
      }
    };

    await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(register2);

    var client = App.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", user1Token);
    });

    var unfollowRequest = new UnfollowProfileRequest { Username = user2Username };
    var (response, _) = await client.DELETEAsync<Unfollow, UnfollowProfileRequest, object>(unfollowRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UnfollowProfile_WithoutAuthentication_ReturnsUnauthorized()
  {
    var unfollowRequest = new UnfollowProfileRequest { Username = "someuser" };
    var (response, _) = await App.Client.DELETEAsync<Unfollow, UnfollowProfileRequest, object>(unfollowRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }
}
