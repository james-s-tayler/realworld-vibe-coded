using Microsoft.Extensions.Configuration;

namespace Server.Web.DevOnly.Configuration;

public sealed class FeatureFlagOverrideSource : IConfigurationSource
{
  public FeatureFlagOverrideProvider Provider { get; } = new();

  public IConfigurationProvider Build(IConfigurationBuilder builder) => Provider;
}
