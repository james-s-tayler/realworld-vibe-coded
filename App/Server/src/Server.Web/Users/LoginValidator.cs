using FastEndpoints;
using FluentValidation;

namespace Server.Web.Users;

public class LoginValidator : Validator<LoginRequest>
{
  public LoginValidator()
  {
    RuleFor(x => x.User.Email)
      .NotEmpty()
      .WithMessage("Email is required.")
      .EmailAddress()
      .WithMessage("Email format is invalid.");

    RuleFor(x => x.User.Password)
      .NotEmpty()
      .WithMessage("Password is required.");
  }
}