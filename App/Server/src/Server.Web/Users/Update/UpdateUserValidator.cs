using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.Resources;
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
        .MaximumLength(ApplicationUser.EmailMaxLength);
    });

    // Username validation - if provided, must be valid
    When(x => x.User.Username != null, () =>
    {
      RuleFor(x => x.User.Username)
        .NotEmpty()
        .MinimumLength(ApplicationUser.UsernameMinLength)
        .MaximumLength(ApplicationUser.UsernameMaxLength);
    });

    // Password validation - if provided, must be valid
    When(x => x.User.Password != null, () =>
    {
      RuleFor(x => x.User.Password)
        .NotEmpty()
        .MinimumLength(ApplicationUser.PasswordMinLength);
    });

    // Bio validation - if provided, must be valid
    When(x => x.User.Bio != null, () =>
    {
      RuleFor(x => x.User.Bio)
        .NotEmpty()
        .MaximumLength(ApplicationUser.BioMaxLength);
    });

    RuleFor(x => x.User.Language)
      .Must(lang => lang == null || i18nSettings.Value.SupportedLanguages.Contains(lang))
      .WithMessage(x => localizer[SharedResource.Keys.UnsupportedLanguage]);
  }
}
