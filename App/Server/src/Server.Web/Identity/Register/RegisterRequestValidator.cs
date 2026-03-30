using FluentValidation;

namespace Server.Web.Identity.Register;

public class RegisterRequestValidator : Validator<RegisterRequest>
{
  public RegisterRequestValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress();

    RuleFor(x => x.Password)
      .NotEmpty();
  }
}
