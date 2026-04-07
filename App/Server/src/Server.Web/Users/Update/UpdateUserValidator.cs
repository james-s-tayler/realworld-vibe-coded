using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Server.Core.IdentityAggregate;
using Server.SharedKernel;
using Server.Web.I18n;

namespace Server.Web.Users.Update;

public class UpdateUserValidator : Validator<UpdateUserRequest>
{
  public UpdateUserValidator(IStringLocalizer<SharedResource> localizer, IOptions<I18nSettings> i18nSettings)
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    // Email validation - if provided, must be valid
    When(x => x.User.Email != null, () =>
    {
      RuleFor(x => x.User.Email)
        .NotEmpty()
        .EmailAddress()
        .MaximumLength(ApplicationUser.EmailMaxLength)
        .OverridePropertyName("email");
    });

    // Username validation - if provided, must be valid
    When(x => x.User.Username != null, () =>
    {
      RuleFor(x => x.User.Username)
        .NotEmpty()
        .MinimumLength(ApplicationUser.UsernameMinLength)
        .MaximumLength(ApplicationUser.UsernameMaxLength)
        .OverridePropertyName("username");
    });

    // Password validation - if provided, must be valid
    When(x => x.User.Password != null, () =>
    {
      RuleFor(x => x.User.Password)
        .NotEmpty()
        .MinimumLength(ApplicationUser.PasswordMinLength)
        .OverridePropertyName("password");
    });

    // Bio validation - if provided, must be valid
    When(x => x.User.Bio != null, () =>
    {
      RuleFor(x => x.User.Bio)
        .NotEmpty()
        .MaximumLength(ApplicationUser.BioMaxLength)
        .OverridePropertyName("bio");
    });

    // Language validation - if provided, must be a supported language
    var supportedLanguages = i18nSettings.Value.SupportedLanguages;
    When(x => x.User.Language != null, () =>
    {
      RuleFor(x => x.User.Language)
        .Must(lang => supportedLanguages.Contains(lang!))
        .WithMessage(x => localizer[SharedResource.Keys.UnsupportedLanguage])
        .OverridePropertyName("language");
    });
  }
}
