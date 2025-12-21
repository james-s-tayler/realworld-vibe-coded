using Server.Web.Profiles;
using Server.Web.Profiles.Follow;
using Server.Web.Profiles.Get;
using Server.Web.Profiles.Unfollow;

namespace Server.FunctionalTests.Profiles;

[Collection("Profiles Integration Tests")]
public class ProfilesTests(ProfilesFixture app) : TestBase<ProfilesFixture>
{
  [Fact]
  public async Task GetProfile_Unauthenticated_ReturnsProfile()
  {
    // Identity API sets username to email by default
    var email = $"test-{Guid.NewGuid()}@example.com";
    var password = "Password123!";

    await IdentityApiHelpers.RegisterUserAsync(app.Client, email, password, TestContext.Current.CancellationToken);

    var request = new GetProfileRequest { Username = email };
    var (response, result) = await app.Client.GETAsync<Get, GetProfileRequest, ProfileResponse>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(email);
    result.Profile.Following.ShouldBeFalse();
  }

  [Fact]
  public async Task GetProfile_Authenticated_NotFollowing_ReturnsProfile()
  {
    // Identity API sets username to email by default
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";
    var password = "Password123!";

    var user1Token = await IdentityApiHelpers.RegisterUserAsync(app.Client, user1Email, password, TestContext.Current.CancellationToken);
    await IdentityApiHelpers.RegisterUserAsync(app.Client, user2Email, password, TestContext.Current.CancellationToken);

    var client = IdentityApiHelpers.CreateAuthenticatedClient(cfg => app.CreateClient(cfg), user1Token);

    var request = new GetProfileRequest { Username = user2Email };
    var (response, result) = await client.GETAsync<Get, GetProfileRequest, ProfileResponse>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(user2Email);
    result.Profile.Following.ShouldBeFalse();
  }

  [Fact]
  public async Task GetProfile_Authenticated_Following_ReturnsProfileWithFollowing()
  {
    // Identity API sets username to email by default
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";
    var password = "Password123!";

    var user1Token = await IdentityApiHelpers.RegisterUserAsync(app.Client, user1Email, password, TestContext.Current.CancellationToken);
    await IdentityApiHelpers.RegisterUserAsync(app.Client, user2Email, password, TestContext.Current.CancellationToken);

    var client = IdentityApiHelpers.CreateAuthenticatedClient(cfg => app.CreateClient(cfg), user1Token);

    var followRequest = new FollowProfileRequest { Username = user2Email };
    await client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    var getRequest = new GetProfileRequest { Username = user2Email };
    var (response, result) = await client.GETAsync<Get, GetProfileRequest, ProfileResponse>(getRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(user2Email);
    result.Profile.Following.ShouldBeTrue();
  }

  [Fact]
  public async Task GetProfile_NonExistentUser_ReturnsNotFound()
  {
    var request = new GetProfileRequest { Username = "nonexistentuser999" };
    var (response, _) = await app.Client.GETAsync<Get, GetProfileRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetProfile_InvalidUsername_ReturnsNotFound()
  {
    var request = new GetProfileRequest { Username = "invalid user!" };
    var (response, _) = await app.Client.GETAsync<Get, GetProfileRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task FollowProfile_WithAuthentication_ReturnsProfileWithFollowing()
  {
    // Identity API sets username to email by default
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";
    var password = "Password123!";

    var user1Token = await IdentityApiHelpers.RegisterUserAsync(app.Client, user1Email, password, TestContext.Current.CancellationToken);
    await IdentityApiHelpers.RegisterUserAsync(app.Client, user2Email, password, TestContext.Current.CancellationToken);

    var client = IdentityApiHelpers.CreateAuthenticatedClient(cfg => app.CreateClient(cfg), user1Token);

    var followRequest = new FollowProfileRequest { Username = user2Email };
    var (response, result) = await client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(user2Email);
    result.Profile.Following.ShouldBeTrue();
  }

  [Fact]
  public async Task FollowProfile_AlreadyFollowing_ReturnsProfileWithFollowing()
  {
    // Identity API sets username to email by default
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";
    var password = "Password123!";

    var user1Token = await IdentityApiHelpers.RegisterUserAsync(app.Client, user1Email, password, TestContext.Current.CancellationToken);
    await IdentityApiHelpers.RegisterUserAsync(app.Client, user2Email, password, TestContext.Current.CancellationToken);

    var client = IdentityApiHelpers.CreateAuthenticatedClient(cfg => app.CreateClient(cfg), user1Token);

    var followRequest = new FollowProfileRequest { Username = user2Email };
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
    var (response, _) = await app.Client.POSTAsync<Follow, FollowProfileRequest, object>(followRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task FollowProfile_NonExistentUser_ReturnsNotFound()
  {
    var (client, _, _) = await IdentityApiHelpers.RegisterUserAndCreateClientAsync(
      app.Client,
      cfg => app.CreateClient(cfg),
      cancellationToken: TestContext.Current.CancellationToken);

    var followRequest = new FollowProfileRequest { Username = "nonexistentuser999" };
    var (response, _) = await client.POSTAsync<Follow, FollowProfileRequest, object>(followRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UnfollowProfile_WithAuthentication_ReturnsProfileWithoutFollowing()
  {
    // Identity API sets username to email by default
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";
    var password = "Password123!";

    var user1Token = await IdentityApiHelpers.RegisterUserAsync(app.Client, user1Email, password, TestContext.Current.CancellationToken);
    await IdentityApiHelpers.RegisterUserAsync(app.Client, user2Email, password, TestContext.Current.CancellationToken);

    var client = IdentityApiHelpers.CreateAuthenticatedClient(cfg => app.CreateClient(cfg), user1Token);

    var followRequest = new FollowProfileRequest { Username = user2Email };
    await client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    var unfollowRequest = new UnfollowProfileRequest { Username = user2Email };
    var (response, result) = await client.DELETEAsync<Unfollow, UnfollowProfileRequest, ProfileResponse>(unfollowRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(user2Email);
    result.Profile.Following.ShouldBeFalse();
  }

  [Fact]
  public async Task UnfollowProfile_NotFollowing_ReturnsErrorDetail()
  {
    // Identity API sets username to email by default
    var user1Email = $"user1-{Guid.NewGuid()}@example.com";
    var user2Email = $"user2-{Guid.NewGuid()}@example.com";
    var password = "Password123!";

    var user1Token = await IdentityApiHelpers.RegisterUserAsync(app.Client, user1Email, password, TestContext.Current.CancellationToken);
    await IdentityApiHelpers.RegisterUserAsync(app.Client, user2Email, password, TestContext.Current.CancellationToken);

    var client = IdentityApiHelpers.CreateAuthenticatedClient(cfg => app.CreateClient(cfg), user1Token);

    var unfollowRequest = new UnfollowProfileRequest { Username = user2Email };
    var (response, _) = await client.DELETEAsync<Unfollow, UnfollowProfileRequest, object>(unfollowRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UnfollowProfile_WithoutAuthentication_ReturnsUnauthorized()
  {
    var unfollowRequest = new UnfollowProfileRequest { Username = "someuser" };
    var (response, _) = await app.Client.DELETEAsync<Unfollow, UnfollowProfileRequest, object>(unfollowRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }
}
