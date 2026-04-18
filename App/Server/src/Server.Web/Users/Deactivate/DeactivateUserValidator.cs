using FluentValidation;

namespace Server.Web.Users.Deactivate;

public class DeactivateUserValidator : Validator<DeactivateUserRequest>
{
  public DeactivateUserValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.UserId)
      .NotEmpty()
      .OverridePropertyName("userId");
  }
}
