using Server.SharedKernel.MediatR;

namespace Server.UseCases.FeatureFlags.List;

public record ListFeatureFlagsQuery : IQuery<FeatureFlagDefinitions>;
