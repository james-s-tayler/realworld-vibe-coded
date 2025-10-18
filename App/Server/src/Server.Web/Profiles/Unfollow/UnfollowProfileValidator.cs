using FluentValidation;

namespace Server.Web.Profiles.Unfollow;

public class UnfollowProfileValidator : Validator<UnfollowProfileRequest>
{
  public UnfollowProfileValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Username)
      .NotEmpty()
      .WithMessage("is required.")
      .OverridePropertyName("username");
  }
}
