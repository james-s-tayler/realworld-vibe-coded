using FluentValidation;

namespace Server.Web.Identity.Login;

public class LoginRequestValidator : Validator<LoginRequest>
{
  public LoginRequestValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Email)
      .NotEmpty()
      .WithMessage("is required.")
      .EmailAddress()
      .WithMessage("is invalid.");

    RuleFor(x => x.Password)
      .NotEmpty()
      .WithMessage("is required.");
  }
}
