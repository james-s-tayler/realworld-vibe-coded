using Server.Web.DevOnly.Endpoints.FeatureFlag;
using Server.Web.FeatureFlags.List;

namespace Server.FunctionalTests.FeatureFlags;

public class FeatureFlagOverrideTests : AppTestBase, IDisposable
{
  public FeatureFlagOverrideTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  public void Dispose()
  {
    Fixture.ClearFeatureFlagOverrides();
  }

  [Fact]
  public async Task SetOverride_EnablesDisabledFlag()
  {
    var (response, result) = await Fixture.Client
      .PUTAsync<SetFeatureFlagOverride, SetFeatureFlagOverrideRequest, CheckFeatureFlagResponse>(
        new SetFeatureFlagOverrideRequest { FeatureName = "DashboardBanner", Enabled = true });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.FeatureName.ShouldBe("DashboardBanner");
    result.IsEnabled.ShouldBeTrue();
  }

  [Fact]
  public async Task SetOverride_DisablesEnabledFlag()
  {
    var (response, result) = await Fixture.Client
      .PUTAsync<SetFeatureFlagOverride, SetFeatureFlagOverrideRequest, CheckFeatureFlagResponse>(
        new SetFeatureFlagOverrideRequest { FeatureName = "SampleFeature", Enabled = false });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.FeatureName.ShouldBe("SampleFeature");
    result.IsEnabled.ShouldBeFalse();
  }

  [Fact]
  public async Task SetOverride_ReflectedInCheckEndpoint()
  {
    await Fixture.Client
      .PUTAsync<SetFeatureFlagOverride, SetFeatureFlagOverrideRequest, CheckFeatureFlagResponse>(
        new SetFeatureFlagOverrideRequest { FeatureName = "DashboardBanner", Enabled = true });

    var (response, result) = await Fixture.Client
      .GETAsync<CheckFeatureFlag, CheckFeatureFlagRequest, CheckFeatureFlagResponse>(
        new CheckFeatureFlagRequest { FeatureName = "DashboardBanner" });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.IsEnabled.ShouldBeTrue();
  }

  [Fact]
  public async Task SetOverride_ReflectedInListEndpoint()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    await Fixture.Client
      .PUTAsync<SetFeatureFlagOverride, SetFeatureFlagOverrideRequest, CheckFeatureFlagResponse>(
        new SetFeatureFlagOverrideRequest { FeatureName = "DashboardBanner", Enabled = true });

    var (response, result) = await user.Client.GETAsync<ListFeatureFlags, FeatureFlagsResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var dashboardBanner = result.FeatureManagement.FeatureFlags.First(f => f.Id == "DashboardBanner");
    dashboardBanner.Enabled.ShouldBeTrue();
  }

  [Fact]
  public async Task ClearOverride_RevertsToConfigDefault()
  {
    await Fixture.Client
      .PUTAsync<SetFeatureFlagOverride, SetFeatureFlagOverrideRequest, CheckFeatureFlagResponse>(
        new SetFeatureFlagOverrideRequest { FeatureName = "DashboardBanner", Enabled = true });

    var (response, result) = await Fixture.Client
      .DELETEAsync<ClearFeatureFlagOverride, CheckFeatureFlagRequest, CheckFeatureFlagResponse>(
        new CheckFeatureFlagRequest { FeatureName = "DashboardBanner" });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.FeatureName.ShouldBe("DashboardBanner");
    result.IsEnabled.ShouldBeFalse();
  }

  [Fact]
  public async Task OverridePersistsAcrossMultipleRequests()
  {
    await Fixture.Client
      .PUTAsync<SetFeatureFlagOverride, SetFeatureFlagOverrideRequest, CheckFeatureFlagResponse>(
        new SetFeatureFlagOverrideRequest { FeatureName = "DashboardBanner", Enabled = true });

    for (var i = 0; i < 3; i++)
    {
      var (response, result) = await Fixture.Client
        .GETAsync<CheckFeatureFlag, CheckFeatureFlagRequest, CheckFeatureFlagResponse>(
          new CheckFeatureFlagRequest { FeatureName = "DashboardBanner" });

      response.StatusCode.ShouldBe(HttpStatusCode.OK);
      result.IsEnabled.ShouldBeTrue();
    }
  }
}
