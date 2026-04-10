using Server.Web.DevOnly.Endpoints.FeatureFlag;

namespace Server.Web.DevOnly.UseCases.FeatureFlag;

#pragma warning disable SRV015 // DevOnly test endpoint
public record SetFeatureFlagOverrideCommand(string FeatureName, bool Enabled) : Server.SharedKernel.MediatR.ICommand<CheckFeatureFlagResponse>;
#pragma warning restore SRV015
