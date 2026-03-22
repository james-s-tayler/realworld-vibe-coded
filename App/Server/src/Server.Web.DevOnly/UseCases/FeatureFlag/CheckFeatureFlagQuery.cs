using Server.SharedKernel.MediatR;
using Server.Web.DevOnly.Endpoints.FeatureFlag;

namespace Server.Web.DevOnly.UseCases.FeatureFlag;

#pragma warning disable SRV015 // DevOnly test endpoint
public record CheckFeatureFlagQuery(string FeatureName) : IQuery<CheckFeatureFlagResponse>;
#pragma warning restore SRV015
