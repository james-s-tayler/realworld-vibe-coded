using FluentValidation;

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
        .OverridePropertyName("email");
    });

    // Username validation - if provided, must be valid
    When(x => x.User.Username != null, () =>
    {
      RuleFor(x => x.User.Username)
        .NotEmpty()
        .WithMessage("is required.")
        .MinimumLength(2)
        .WithMessage("must be at least 2 characters.")
        .MaximumLength(100)
        .WithMessage("cannot exceed 100 characters.")
        .OverridePropertyName("username");
    });

    // Password validation - if provided, must be valid
    When(x => x.User.Password != null, () =>
    {
      RuleFor(x => x.User.Password)
        .NotEmpty()
        .WithMessage("is required.")
        .MinimumLength(6)
        .WithMessage("must be at least 6 characters.")
        .OverridePropertyName("password");
    });

    // Bio validation - if provided, must be valid
    When(x => x.User.Bio != null, () =>
    {
      RuleFor(x => x.User.Bio)
        .NotEmpty()
        .WithMessage("is required.")
        .MaximumLength(1000)
        .WithMessage("cannot exceed 1000 characters.")
        .OverridePropertyName("bio");
    });
  }
}
