using FluentValidation;
using Server.Core.IdentityAggregate;

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
        .MaximumLength(ApplicationUser.EmailMaxLength)
        .WithMessage($"cannot exceed {ApplicationUser.EmailMaxLength} characters.")
        .OverridePropertyName("email");
    });

    // Username validation - if provided, must be valid
    When(x => x.User.Username != null, () =>
    {
      RuleFor(x => x.User.Username)
        .NotEmpty()
        .WithMessage("is required.")
        .MinimumLength(ApplicationUser.UsernameMinLength)
        .WithMessage($"must be at least {ApplicationUser.UsernameMinLength} characters.")
        .MaximumLength(ApplicationUser.UsernameMaxLength)
        .WithMessage($"cannot exceed {ApplicationUser.UsernameMaxLength} characters.")
        .OverridePropertyName("username");
    });

    // Password validation - if provided, must be valid
    When(x => x.User.Password != null, () =>
    {
      RuleFor(x => x.User.Password)
        .NotEmpty()
        .WithMessage("is required.")
        .MinimumLength(ApplicationUser.PasswordMinLength)
        .WithMessage($"must be at least {ApplicationUser.PasswordMinLength} characters.")
        .OverridePropertyName("password");
    });

    // Bio validation - if provided, must be valid
    When(x => x.User.Bio != null, () =>
    {
      RuleFor(x => x.User.Bio)
        .NotEmpty()
        .WithMessage("is required.")
        .MaximumLength(ApplicationUser.BioMaxLength)
        .WithMessage($"cannot exceed {ApplicationUser.BioMaxLength} characters.")
        .OverridePropertyName("bio");
    });
  }
}
