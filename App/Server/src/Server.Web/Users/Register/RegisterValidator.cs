using FluentValidation;
using Server.Core.UserAggregate;

namespace Server.Web.Users.Register;

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
      .MaximumLength(User.EmailMaxLength)
      .WithMessage($"cannot exceed {User.EmailMaxLength} characters.")
      .OverridePropertyName("email");

    RuleFor(x => x.User.Username)
      .NotEmpty()
      .WithMessage("is required.")
      .MinimumLength(User.UsernameMinLength)
      .WithMessage($"must be at least {User.UsernameMinLength} characters.")
      .MaximumLength(User.UsernameMaxLength)
      .WithMessage($"cannot exceed {User.UsernameMaxLength} characters.")
      .OverridePropertyName("username");

    RuleFor(x => x.User.Password)
      .NotEmpty()
      .WithMessage("is required.")
      .MinimumLength(User.PasswordMinLength)
      .WithMessage($"must be at least {User.PasswordMinLength} characters.")
      .OverridePropertyName("password");
  }
}
