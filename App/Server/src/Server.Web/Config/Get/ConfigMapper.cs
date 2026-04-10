using Server.UseCases.Config.Get;

namespace Server.Web.Config.Get;

public class ConfigMapper : ResponseMapper<ConfigResponse, ConfigDefinitions>
{
  public override Task<ConfigResponse> FromEntityAsync(ConfigDefinitions definitions, CancellationToken ct)
  {
    var response = new ConfigResponse
    {
      FeatureFlagRefreshIntervalSeconds = definitions.FeatureFlagRefreshIntervalSeconds,
    };

    return Task.FromResult(response);
  }
}
