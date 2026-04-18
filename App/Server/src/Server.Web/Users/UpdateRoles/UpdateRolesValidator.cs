using FluentValidation;
using Microsoft.Extensions.Localization;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.Resources;

namespace Server.Web.Users.UpdateRoles;

public class UpdateRolesValidator : Validator<UpdateRolesRequest>
{
  private static readonly HashSet<string> AllowedRoles =
  [
    DefaultRoles.Admin,
    DefaultRoles.Author,
    DefaultRoles.Moderator,
  ];

  public UpdateRolesValidator(IStringLocalizer<SharedResource> localizer)
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Roles)
      .NotEmpty();

    RuleForEach(x => x.Roles)
      .Must(role => AllowedRoles.Contains(role))
      .WithMessage((_, role) => string.Format(localizer[SharedResource.Keys.InvalidAssignableRole], role));
  }
}
