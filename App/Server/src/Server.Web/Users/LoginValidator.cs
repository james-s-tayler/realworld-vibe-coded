using FluentValidation;

namespace Server.Web.Users;

public class LoginValidator : Validator<LoginRequest>
{
  public LoginValidator()
  {
    RuleFor(x => x.User.Email)
      .NotEmpty()
      .WithMessage("is required.")
      .EmailAddress()
      .WithMessage("is invalid.")
      .OverridePropertyName("email");

    RuleFor(x => x.User.Password)
      .NotEmpty()
      .WithMessage("is required.")
      .OverridePropertyName("password");
  }
}
