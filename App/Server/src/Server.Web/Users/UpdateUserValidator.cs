using FastEndpoints;
using FluentValidation;

namespace Server.Web.Users;

public class UpdateUserValidator : Validator<UpdateUserRequest>
{
  public UpdateUserValidator()
  {
    When(x => !string.IsNullOrEmpty(x.User.Email), () =>
    {
      RuleFor(x => x.User.Email)
        .EmailAddress()
        .WithMessage("is invalid.")
        .OverridePropertyName("email");
    });

    When(x => !string.IsNullOrEmpty(x.User.Username), () =>
    {
      RuleFor(x => x.User.Username)
        .MinimumLength(2)
        .WithMessage("must be at least 2 characters.")
        .MaximumLength(100)
        .WithMessage("cannot exceed 100 characters.")
        .OverridePropertyName("username");
    });

    When(x => !string.IsNullOrEmpty(x.User.Password), () =>
    {
      RuleFor(x => x.User.Password)
        .MinimumLength(6)
        .WithMessage("must be at least 6 characters.")
        .OverridePropertyName("password");
    });

    When(x => x.User.Bio != null, () =>
    {
      RuleFor(x => x.User.Bio)
        .MaximumLength(1000)
        .WithMessage("cannot exceed 1000 characters.")
        .OverridePropertyName("bio");
    });
  }
}