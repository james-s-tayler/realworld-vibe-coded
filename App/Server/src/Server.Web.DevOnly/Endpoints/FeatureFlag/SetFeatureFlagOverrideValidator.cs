namespace Server.Web.DevOnly.Endpoints.FeatureFlag;

public class SetFeatureFlagOverrideValidator : Validator<SetFeatureFlagOverrideRequest>
{
  public SetFeatureFlagOverrideValidator()
  {
    RuleFor(x => x.FeatureName)
      .NotEmpty();
  }
}
