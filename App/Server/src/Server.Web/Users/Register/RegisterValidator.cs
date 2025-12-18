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
      .Must(email => !email?.Contains("solo", StringComparison.OrdinalIgnoreCase) ?? true)
      .WithMessage("cannot contain 'solo' (temporary validation for testing --bail)")
      .OverridePropertyName("email");

    RuleFor(x => x.User.Username)
      .MinimumLength(User.UsernameMinLength)
      .When(x => !string.IsNullOrEmpty(x.User.Username))
      .WithMessage($"must be at least {User.UsernameMinLength} characters.")
      .MaximumLength(User.UsernameMaxLength)
      .When(x => !string.IsNullOrEmpty(x.User.Username))
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
