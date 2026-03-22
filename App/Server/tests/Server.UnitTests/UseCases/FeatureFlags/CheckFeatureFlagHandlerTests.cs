using Server.SharedKernel.Interfaces;
using Server.Web.DevOnly.UseCases.FeatureFlag;

namespace Server.UnitTests.UseCases.FeatureFlags;

public class CheckFeatureFlagHandlerTests
{
  private readonly IFeatureFlagService _featureFlagService = Substitute.For<IFeatureFlagService>();
  private readonly CheckFeatureFlagHandler _sut;

  public CheckFeatureFlagHandlerTests()
  {
    _sut = new CheckFeatureFlagHandler(_featureFlagService);
  }

  [Fact]
  public async Task Handle_WhenFeatureEnabled_ReturnsSuccessWithEnabledTrue()
  {
    _featureFlagService.IsEnabledAsync("SampleFeature").Returns(true);

    var result = await _sut.Handle(new CheckFeatureFlagQuery("SampleFeature"), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.FeatureName.ShouldBe("SampleFeature");
    result.Value.IsEnabled.ShouldBeTrue();
  }

  [Fact]
  public async Task Handle_WhenFeatureDisabled_ReturnsSuccessWithEnabledFalse()
  {
    _featureFlagService.IsEnabledAsync("SampleFeature").Returns(false);

    var result = await _sut.Handle(new CheckFeatureFlagQuery("SampleFeature"), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.FeatureName.ShouldBe("SampleFeature");
    result.Value.IsEnabled.ShouldBeFalse();
  }

  [Fact]
  public async Task Handle_DelegatesToFeatureFlagService()
  {
    _featureFlagService.IsEnabledAsync("MyFeature").Returns(false);

    await _sut.Handle(new CheckFeatureFlagQuery("MyFeature"), CancellationToken.None);

    await _featureFlagService.Received(1).IsEnabledAsync("MyFeature");
  }
}
