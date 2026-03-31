using Server.Web.FeatureFlags.List;

namespace Server.FunctionalTests.FeatureFlags;

public class ListFeatureFlagsTests : AppTestBase
{
  public ListFeatureFlagsTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task ListFeatureFlags_ReturnsV2FormatWithClientVisibleFlags()
  {
    var (response, result) = await Fixture.Client.GETAsync<ListFeatureFlags, FeatureFlagsResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.FeatureManagement.ShouldNotBeNull();
    result.FeatureManagement.FeatureFlags.ShouldNotBeNull();
    result.FeatureManagement.FeatureFlags.Count.ShouldBe(1);

    var dashboardBanner = result.FeatureManagement.FeatureFlags.First();
    dashboardBanner.Id.ShouldBe("DashboardBanner");
    dashboardBanner.Enabled.ShouldBeFalse();
  }

  [Fact]
  public async Task ListFeatureFlags_DoesNotExposeNonClientVisibleFlags()
  {
    var (_, result) = await Fixture.Client.GETAsync<ListFeatureFlags, FeatureFlagsResponse>();

    var flagIds = result.FeatureManagement.FeatureFlags.Select(f => f.Id).ToList();
    flagIds.ShouldNotContain("SampleFeature");
    flagIds.ShouldNotContain("DisabledFeature");
  }

  [Fact]
  public async Task ListFeatureFlags_IsAccessibleWithoutAuthentication()
  {
    var (response, _) = await Fixture.Client.GETAsync<ListFeatureFlags, FeatureFlagsResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }
}
