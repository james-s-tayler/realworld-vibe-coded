using FluentValidation;

namespace Server.Web.Users;

public class RegisterValidator : Validator<RegisterRequest>
{
  public RegisterValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.User.Email)
      .NotEmpty()
      .WithMessage("is required.")
      .EmailAddress()
      .WithMessage("is invalid.")
      .OverridePropertyName("email");

    RuleFor(x => x.User.Username)
      .NotEmpty()
      .WithMessage("is required.")
      .MinimumLength(2)
      .WithMessage("must be at least 2 characters.")
      .MaximumLength(100)
      .WithMessage("cannot exceed 100 characters.")
      .OverridePropertyName("username");

    RuleFor(x => x.User.Password)
      .NotEmpty()
      .WithMessage("is required.")
      .MinimumLength(6)
      .WithMessage("must be at least 6 characters.")
      .OverridePropertyName("password");
  }
}
