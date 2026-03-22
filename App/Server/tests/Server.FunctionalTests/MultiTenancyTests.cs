using Server.Web.Profiles.Get;
using Server.Web.Users.Update;

namespace Server.FunctionalTests;

public class MultiTenancyTests : AppTestBase
{
  public MultiTenancyTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task Users_AreIsolated_BetweenTenants()
  {
    // Arrange
    var tenant1 = await Fixture.RegisterTenantAsync();
    var tenant2 = await Fixture.RegisterTenantAsync();

    var tenant1UserEmail = tenant1.Users[0].Email;

    // Act
    var getProfileRequest = new GetProfileRequest { Username = tenant1UserEmail };
    var (response, _) = await tenant2.Users[0].Client.GETAsync<Get, GetProfileRequest, object>(getProfileRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DuplicateUsernames_AreAllowed_BetweenTenants()
  {
    // Arrange
    var tenant1 = await Fixture.RegisterTenantAsync();
    var tenant2 = await Fixture.RegisterTenantAsync();

    var sharedUsername = $"shared-username-{Guid.NewGuid()}";

    var updateRequest1 = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = sharedUsername,
      },
    };

    await tenant1.Users[0].Client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest1);

    // Act
    var updateRequest2 = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = sharedUsername,
      },
    };

    var (response, result) = await tenant2.Users[0].Client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest2);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.ShouldNotBeNull();
    result.User.Username.ShouldBe(sharedUsername);
  }
}
