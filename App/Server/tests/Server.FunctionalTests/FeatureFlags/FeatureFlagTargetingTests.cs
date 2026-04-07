using Server.Web.DevOnly.Endpoints.FeatureFlag;

namespace Server.FunctionalTests.FeatureFlags;

public class FeatureFlagTargetingTests : AppTestBase
{
  private const int TenantCount = 15;

  public FeatureFlagTargetingTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task TargetingTestFlag_With50PercentRollout_ProducesDifferentiatedResults()
  {
    var tenants = new List<RegisteredTenant>();
    for (var i = 0; i < TenantCount; i++)
    {
      tenants.Add(await Fixture.RegisterTenantAsync());
    }

    var results = new List<bool>();
    foreach (var tenant in tenants)
    {
      var (response, result) = await tenant.GetTenantOwner().Client
        .GETAsync<CheckFeatureFlag, CheckFeatureFlagRequest, CheckFeatureFlagResponse>(
          new CheckFeatureFlagRequest { FeatureName = "TargetingTestFlag" });

      response.StatusCode.ShouldBe(HttpStatusCode.OK);
      results.Add(result.IsEnabled);
    }

    results.ShouldContain(true, "At least one tenant should have the flag enabled (50% rollout with 15 tenants)");
    results.ShouldContain(false, "At least one tenant should have the flag disabled (50% rollout with 15 tenants)");
  }

  [Fact]
  public async Task TargetingTestFlag_SameTenant_ReturnsDeterministicResult()
  {
    var tenant = await Fixture.RegisterTenantAsync();

    var (_, first) = await tenant.GetTenantOwner().Client
      .GETAsync<CheckFeatureFlag, CheckFeatureFlagRequest, CheckFeatureFlagResponse>(
        new CheckFeatureFlagRequest { FeatureName = "TargetingTestFlag" });

    var (_, second) = await tenant.GetTenantOwner().Client
      .GETAsync<CheckFeatureFlag, CheckFeatureFlagRequest, CheckFeatureFlagResponse>(
        new CheckFeatureFlagRequest { FeatureName = "TargetingTestFlag" });

    first.IsEnabled.ShouldBe(second.IsEnabled, "Same tenant should always get the same targeting result");
  }
}
