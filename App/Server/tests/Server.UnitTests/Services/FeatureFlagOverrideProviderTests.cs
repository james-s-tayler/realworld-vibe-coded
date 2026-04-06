using Microsoft.Extensions.Configuration;
using Server.Web.DevOnly.Configuration;

namespace Server.UnitTests.Services;

public class FeatureFlagOverrideProviderTests
{
  private readonly FeatureFlagOverrideProvider _sut = new();

  [Fact]
  public void SetOverride_WritesCorrectConfigKey()
  {
    var config = BuildConfiguration();

    _sut.SetOverride("DashboardBanner", true, config);

    _sut.TryGet("feature_management:feature_flags:0:enabled", out var value);
    value.ShouldBe("True");
  }

  [Fact]
  public void SetOverride_CanDisableFlag()
  {
    var config = BuildConfiguration();

    _sut.SetOverride("SampleFeature", false, config);

    _sut.TryGet("feature_management:feature_flags:1:enabled", out var value);
    value.ShouldBe("False");
  }

  [Fact]
  public void ClearOverride_RemovesKey()
  {
    var config = BuildConfiguration();

    _sut.SetOverride("DashboardBanner", true, config);
    _sut.ClearOverride("DashboardBanner", config);

    _sut.TryGet("feature_management:feature_flags:0:enabled", out _).ShouldBeFalse();
  }

  [Fact]
  public void ClearAllOverrides_RemovesAllKeys()
  {
    var config = BuildConfiguration();

    _sut.SetOverride("DashboardBanner", true, config);
    _sut.SetOverride("SampleFeature", false, config);

    _sut.ClearAllOverrides();

    _sut.TryGet("feature_management:feature_flags:0:enabled", out _).ShouldBeFalse();
    _sut.TryGet("feature_management:feature_flags:1:enabled", out _).ShouldBeFalse();
  }

  [Fact]
  public void SetOverride_NonExistentFlag_IsNoOp()
  {
    var config = BuildConfiguration();

    _sut.SetOverride("NonExistentFlag", true, config);

    _sut.TryGet("feature_management:feature_flags:0:enabled", out _).ShouldBeFalse();
    _sut.TryGet("feature_management:feature_flags:1:enabled", out _).ShouldBeFalse();
  }

  [Fact]
  public void ClearOverride_NonExistentFlag_IsNoOp()
  {
    var config = BuildConfiguration();

    // Should not throw
    _sut.ClearOverride("NonExistentFlag", config);
  }

  private static IConfiguration BuildConfiguration()
  {
    var config = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["feature_management:feature_flags:0:id"] = "DashboardBanner",
        ["feature_management:feature_flags:0:enabled"] = "false",
        ["feature_management:feature_flags:1:id"] = "SampleFeature",
        ["feature_management:feature_flags:1:enabled"] = "true",
      })
      .Build();
    return config;
  }
}
