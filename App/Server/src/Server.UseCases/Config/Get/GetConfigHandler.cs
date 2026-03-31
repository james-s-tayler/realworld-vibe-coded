using Microsoft.Extensions.Options;
using Server.SharedKernel.FeatureFlags;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Config.Get;

public class GetConfigHandler(IOptions<FeatureFlagSettings> featureFlagSettings)
  : IQueryHandler<GetConfigQuery, ConfigDefinitions>
{
  public Task<Result<ConfigDefinitions>> Handle(GetConfigQuery request, CancellationToken cancellationToken)
  {
    return Task.FromResult(Result<ConfigDefinitions>.Success(
      new ConfigDefinitions(featureFlagSettings.Value.RefreshIntervalSeconds)));
  }
}
