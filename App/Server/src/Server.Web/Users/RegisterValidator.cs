using FastEndpoints;
using FluentValidation;

namespace Server.Web.Users;

public class RegisterValidator : Validator<RegisterRequest>
{
  public RegisterValidator()
  {
    RuleFor(x => x.User.Email)
      .NotEmpty()
      .WithMessage("Email is required.")
      .EmailAddress()
      .WithMessage("Email format is invalid.");

    RuleFor(x => x.User.Username)
      .NotEmpty()
      .WithMessage("Username is required.")
      .MinimumLength(2)
      .WithMessage("Username must be at least 2 characters.")
      .MaximumLength(100)
      .WithMessage("Username cannot exceed 100 characters.");

    RuleFor(x => x.User.Password)
      .NotEmpty()
      .WithMessage("Password is required.")
      .MinimumLength(6)
      .WithMessage("Password must be at least 6 characters.");
  }
}