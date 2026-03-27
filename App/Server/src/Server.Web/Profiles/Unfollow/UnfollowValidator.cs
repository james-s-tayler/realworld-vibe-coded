using FluentValidation;

namespace Server.Web.Profiles.Unfollow;

public class UnfollowValidator : Validator<UnfollowRequest>
{
  public UnfollowValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Username)
      .NotEmpty().WithMessage("is required.")
      .OverridePropertyName("username");
  }
}
