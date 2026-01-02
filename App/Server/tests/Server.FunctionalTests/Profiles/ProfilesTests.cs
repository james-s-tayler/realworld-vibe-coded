using Server.Web.Profiles;
using Server.Web.Profiles.Follow;
using Server.Web.Profiles.Get;
using Server.Web.Profiles.Unfollow;

namespace Server.FunctionalTests.Profiles;

[Collection("Profiles Integration Tests")]
public class ProfilesTests : AppTestBase<ProfilesFixture>
{
  public ProfilesTests(ProfilesFixture fixture) : base(fixture)
  {
  }

  [Fact]
  public async Task GetProfile_Unauthenticated_ReturnsUnauthorized()
  {
    // Identity API sets username to email by default
    var email = $"test-{Guid.NewGuid()}@example.com";
    var password = "Password123!";

#pragma warning disable SRV007 // Calling ApiFixtureBase helper that internally uses HttpClient for Identity API
    await Fixture.RegisterTenantUserAsync(email, password, TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    var request = new GetProfileRequest { Username = email };
    var (response, _) = await Fixture.Client.GETAsync<Get, GetProfileRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetProfile_Authenticated_NotFollowing_ReturnsProfile()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2, TestContext.Current.CancellationToken);

    // Act
    var request = new GetProfileRequest { Username = tenant.Users[1].Email };
    var (response, result) = await tenant.Users[0].Client.GETAsync<Get, GetProfileRequest, ProfileResponse>(request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(tenant.Users[1].Email);
    result.Profile.Following.ShouldBeFalse();
  }

  [Fact]
  public async Task GetProfile_Authenticated_Following_ReturnsProfileWithFollowing()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2, TestContext.Current.CancellationToken);

    var followRequest = new FollowProfileRequest { Username = tenant.Users[1].Email };
    await tenant.Users[0].Client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    // Act
    var getRequest = new GetProfileRequest { Username = tenant.Users[1].Email };
    var (response, result) = await tenant.Users[0].Client.GETAsync<Get, GetProfileRequest, ProfileResponse>(getRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(tenant.Users[1].Email);
    result.Profile.Following.ShouldBeTrue();
  }

  [Fact]
  public async Task GetProfile_NonExistentUser_ReturnsNotFound()
  {
    var request = new GetProfileRequest { Username = "nonexistentuser999" };
    var (response, _) = await Fixture.AuthenticatedClient.GETAsync<Get, GetProfileRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetProfile_InvalidUsername_ReturnsNotFound()
  {
    var request = new GetProfileRequest { Username = "invalid user!" };
    var (response, _) = await Fixture.AuthenticatedClient.GETAsync<Get, GetProfileRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task FollowProfile_WithAuthentication_ReturnsProfileWithFollowing()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2, TestContext.Current.CancellationToken);

    // Act
    var followRequest = new FollowProfileRequest { Username = tenant.Users[1].Email };
    var (response, result) = await tenant.Users[0].Client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(tenant.Users[1].Email);
    result.Profile.Following.ShouldBeTrue();
  }

  [Fact]
  public async Task FollowProfile_AlreadyFollowing_ReturnsProfileWithFollowing()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2, TestContext.Current.CancellationToken);

    var followRequest = new FollowProfileRequest { Username = tenant.Users[1].Email };
    await tenant.Users[0].Client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    // Act
    var (response, result) = await tenant.Users[0].Client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Following.ShouldBeTrue();
  }

  [Fact]
  public async Task FollowProfile_WithoutAuthentication_ReturnsUnauthorized()
  {
    var followRequest = new FollowProfileRequest { Username = "someuser" };
    var (response, _) = await Fixture.Client.POSTAsync<Follow, FollowProfileRequest, object>(followRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task FollowProfile_NonExistentUser_ReturnsNotFound()
  {
    var (client, _, _) = await Fixture.RegisterTenantAndCreateClientAsync(
      cancellationToken: TestContext.Current.CancellationToken);

    var followRequest = new FollowProfileRequest { Username = "nonexistentuser999" };
    var (response, _) = await client.POSTAsync<Follow, FollowProfileRequest, object>(followRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UnfollowProfile_WithAuthentication_ReturnsProfileWithoutFollowing()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2, TestContext.Current.CancellationToken);

    var followRequest = new FollowProfileRequest { Username = tenant.Users[1].Email };
    await tenant.Users[0].Client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    // Act
    var unfollowRequest = new UnfollowProfileRequest { Username = tenant.Users[1].Email };
    var (response, result) = await tenant.Users[0].Client.DELETEAsync<Unfollow, UnfollowProfileRequest, ProfileResponse>(unfollowRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(tenant.Users[1].Email);
    result.Profile.Following.ShouldBeFalse();
  }

  [Fact]
  public async Task UnfollowProfile_NotFollowing_ReturnsErrorDetail()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2, TestContext.Current.CancellationToken);

    // Act
    var unfollowRequest = new UnfollowProfileRequest { Username = tenant.Users[1].Email };
    var (response, _) = await tenant.Users[0].Client.DELETEAsync<Unfollow, UnfollowProfileRequest, object>(unfollowRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UnfollowProfile_WithoutAuthentication_ReturnsUnauthorized()
  {
    var unfollowRequest = new UnfollowProfileRequest { Username = "someuser" };
    var (response, _) = await Fixture.Client.DELETEAsync<Unfollow, UnfollowProfileRequest, object>(unfollowRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }
}
