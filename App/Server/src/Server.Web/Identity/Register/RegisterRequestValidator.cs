using FluentValidation;

namespace Server.Web.Identity.Register;

public class RegisterRequestValidator : Validator<RegisterRequest>
{
  public RegisterRequestValidator()
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
