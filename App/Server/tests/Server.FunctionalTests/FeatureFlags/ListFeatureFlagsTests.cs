using Server.Web.FeatureFlags.List;

namespace Server.FunctionalTests.FeatureFlags;

public class ListFeatureFlagsTests : AppTestBase
{
  public ListFeatureFlagsTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task ListFeatureFlags_ReturnsV2FormatWithEmptyClientVisibleFlags()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var (response, result) = await user.Client.GETAsync<ListFeatureFlags, FeatureFlagsResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.FeatureManagement.ShouldNotBeNull();
    result.FeatureManagement.FeatureFlags.ShouldNotBeNull();
    result.FeatureManagement.FeatureFlags.Count.ShouldBe(0);
  }

  [Fact]
  public async Task ListFeatureFlags_DoesNotExposeNonClientVisibleFlags()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var (_, result) = await user.Client.GETAsync<ListFeatureFlags, FeatureFlagsResponse>();

    var flagIds = result.FeatureManagement.FeatureFlags.Select(f => f.Id).ToList();
    flagIds.ShouldNotContain("SampleFeature");
    flagIds.ShouldNotContain("DisabledFeature");
  }

  [Fact]
  public async Task ListFeatureFlags_WithoutAuthentication_ReturnsUnauthorized()
  {
    var (response, _) = await Fixture.Client.GETAsync<ListFeatureFlags, object>();

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }
}
