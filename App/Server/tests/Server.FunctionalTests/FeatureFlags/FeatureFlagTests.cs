using Server.Web.DevOnly.Endpoints.FeatureFlag;

namespace Server.FunctionalTests.FeatureFlags;

public class FeatureFlagTests : AppTestBase
{
  public FeatureFlagTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task CheckFeatureFlag_SampleFeature_ReturnsEnabled()
  {
    var (response, result) = await Fixture.Client
      .GETAsync<CheckFeatureFlag, CheckFeatureFlagRequest, CheckFeatureFlagResponse>(
        new CheckFeatureFlagRequest { FeatureName = "SampleFeature" });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.FeatureName.ShouldBe("SampleFeature");
    result.IsEnabled.ShouldBeTrue();
  }

  [Fact]
  public async Task CheckFeatureFlag_UnknownFeature_ReturnsDisabled()
  {
    var (response, result) = await Fixture.Client
      .GETAsync<CheckFeatureFlag, CheckFeatureFlagRequest, CheckFeatureFlagResponse>(
        new CheckFeatureFlagRequest { FeatureName = "NonExistentFeature" });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.FeatureName.ShouldBe("NonExistentFeature");
    result.IsEnabled.ShouldBeFalse();
  }

  [Fact]
  public async Task CheckFeatureFlag_DisabledFeature_ReturnsDisabled()
  {
    var (response, result) = await Fixture.Client
      .GETAsync<CheckFeatureFlag, CheckFeatureFlagRequest, CheckFeatureFlagResponse>(
        new CheckFeatureFlagRequest { FeatureName = "DisabledFeature" });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.FeatureName.ShouldBe("DisabledFeature");
    result.IsEnabled.ShouldBeFalse();
  }
}
