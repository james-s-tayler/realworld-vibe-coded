using FluentValidation;

namespace Server.Web.Profiles.Follow;

public class FollowProfileValidator : Validator<FollowProfileRequest>
{
  public FollowProfileValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Username)
      .NotEmpty()
      .WithMessage("can't be blank")
      .OverridePropertyName("username");
  }
}
