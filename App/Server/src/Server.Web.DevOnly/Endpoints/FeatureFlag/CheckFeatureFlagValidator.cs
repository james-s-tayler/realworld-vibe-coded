namespace Server.Web.DevOnly.Endpoints.FeatureFlag;

public class CheckFeatureFlagValidator : Validator<CheckFeatureFlagRequest>
{
  public CheckFeatureFlagValidator()
  {
    RuleFor(x => x.FeatureName)
      .NotEmpty();
  }
}
