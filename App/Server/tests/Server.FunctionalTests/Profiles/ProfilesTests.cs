using Server.Web.Profiles;
using Server.Web.Profiles.Get;

namespace Server.FunctionalTests.Profiles;

public class ProfilesTests : AppTestBase
{
  public ProfilesTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task GetProfile_Unauthenticated_ReturnsUnauthorized()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();
    var request = new GetProfileRequest { Username = tenant.Users[0].Email };

    // Act
    var (response, _) = await Fixture.Client.GETAsync<Get, GetProfileRequest, object>(request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetProfile_Authenticated_ReturnsProfile()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);

    // Act
    var request = new GetProfileRequest { Username = tenant.Users[1].Email };
    var (response, result) = await tenant.Users[0].Client.GETAsync<Get, GetProfileRequest, ProfileResponse>(request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Profile.ShouldNotBeNull();
    result.Profile.Username.ShouldBe(tenant.Users[1].Email);
  }

  [Fact]
  public async Task GetProfile_NonExistentUser_ReturnsNotFound()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();
    var request = new GetProfileRequest { Username = "nonexistentuser999" };

    // Act
    var (response, _) = await tenant.Users.First().Client.GETAsync<Get, GetProfileRequest, object>(request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetProfile_InvalidUsername_ReturnsNotFound()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();
    var request = new GetProfileRequest { Username = "invalid user!" };

    // Act
    var (response, _) = await tenant.Users.First().Client.GETAsync<Get, GetProfileRequest, object>(request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }
}
