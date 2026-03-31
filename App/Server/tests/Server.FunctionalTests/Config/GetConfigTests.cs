using Server.Web.Config.Get;

namespace Server.FunctionalTests.Config;

public class GetConfigTests : AppTestBase
{
  public GetConfigTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task GetConfig_ReturnsFeatureFlagRefreshInterval()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var (response, result) = await user.Client.GETAsync<GetConfig, ConfigResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.FeatureFlagRefreshIntervalSeconds.ShouldBe(30);
  }

  [Fact]
  public async Task GetConfig_WithoutAuthentication_ReturnsUnauthorized()
  {
    var (response, _) = await Fixture.Client.GETAsync<GetConfig, object>();

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }
}
