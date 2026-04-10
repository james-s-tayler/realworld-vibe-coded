using FluentValidation;

namespace Server.Web.Identity.Login;

public class LoginRequestValidator : Validator<LoginRequest>
{
  public LoginRequestValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress();

    RuleFor(x => x.Password)
      .NotEmpty();
  }
}
