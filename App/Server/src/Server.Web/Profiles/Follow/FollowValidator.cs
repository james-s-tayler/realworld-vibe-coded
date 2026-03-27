using FluentValidation;

namespace Server.Web.Profiles.Follow;

public class FollowValidator : Validator<FollowRequest>
{
  public FollowValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Username)
      .NotEmpty().WithMessage("is required.")
      .OverridePropertyName("username");
  }
}
