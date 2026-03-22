using Microsoft.FeatureManagement;
using Server.Web.Services;

namespace Server.UnitTests.Services;

public class FeatureFlagServiceTests
{
  private readonly IFeatureManager _featureManager = Substitute.For<IFeatureManager>();
  private readonly FeatureFlagService _sut;

  public FeatureFlagServiceTests()
  {
    _sut = new FeatureFlagService(_featureManager);
  }

  [Fact]
  public async Task IsEnabledAsync_WhenFeatureEnabled_ReturnsTrue()
  {
    _featureManager.IsEnabledAsync("TestFeature").Returns(true);

    var result = await _sut.IsEnabledAsync("TestFeature");

    result.ShouldBeTrue();
  }

  [Fact]
  public async Task IsEnabledAsync_WhenFeatureDisabled_ReturnsFalse()
  {
    _featureManager.IsEnabledAsync("TestFeature").Returns(false);

    var result = await _sut.IsEnabledAsync("TestFeature");

    result.ShouldBeFalse();
  }

  [Fact]
  public async Task IsEnabledAsync_DelegatesToFeatureManager()
  {
    _featureManager.IsEnabledAsync("SomeFeature").Returns(true);

    await _sut.IsEnabledAsync("SomeFeature");

    await _featureManager.Received(1).IsEnabledAsync("SomeFeature");
  }
}
