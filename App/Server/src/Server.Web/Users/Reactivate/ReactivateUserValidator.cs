using FluentValidation;

namespace Server.Web.Users.Reactivate;

public class ReactivateUserValidator : Validator<ReactivateUserRequest>
{
  public ReactivateUserValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.UserId)
      .NotEmpty()
      .OverridePropertyName("userId");
  }
}
