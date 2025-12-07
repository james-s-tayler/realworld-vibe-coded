using FluentValidation;
using Server.Core.UserAggregate;

namespace Server.Web.Users.Update;

public class UpdateUserValidator : Validator<UpdateUserRequest>
{
  public UpdateUserValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    // Email validation - if provided, must be valid
    When(x => x.User.Email != null, () =>
    {
      RuleFor(x => x.User.Email)
        .NotEmpty()
        .WithMessage("is required.")
        .EmailAddress()
        .WithMessage("is invalid.")
        .MaximumLength(User.EmailMaxLength)
        .WithMessage($"cannot exceed {User.EmailMaxLength} characters.")
        .OverridePropertyName("email");
    });

    // Username validation - if provided, must be valid
    When(x => x.User.Username != null, () =>
    {
      RuleFor(x => x.User.Username)
        .NotEmpty()
        .WithMessage("is required.")
        .MinimumLength(User.UsernameMinLength)
        .WithMessage($"must be at least {User.UsernameMinLength} characters.")
        .MaximumLength(User.UsernameMaxLength)
        .WithMessage($"cannot exceed {User.UsernameMaxLength} characters.")
        .OverridePropertyName("username");
    });

    // Password validation - if provided, must be valid
    When(x => x.User.Password != null, () =>
    {
      RuleFor(x => x.User.Password)
        .NotEmpty()
        .WithMessage("is required.")
        .MinimumLength(User.PasswordMinLength)
        .WithMessage($"must be at least {User.PasswordMinLength} characters.")
        .OverridePropertyName("password");
    });

    // Bio validation - if provided, must be valid
    When(x => x.User.Bio != null, () =>
    {
      RuleFor(x => x.User.Bio)
        .NotEmpty()
        .WithMessage("is required.")
        .MaximumLength(User.BioMaxLength)
        .WithMessage($"cannot exceed {User.BioMaxLength} characters.")
        .OverridePropertyName("bio");
    });
  }
}
