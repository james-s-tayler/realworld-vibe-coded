using FluentValidation;
using Microsoft.Extensions.Localization;
using Server.Core.IdentityAggregate;
using Server.SharedKernel;

namespace Server.Web.Users.UpdateRoles;

public class UpdateRolesValidator : Validator<UpdateRolesRequest>
{
  private static readonly HashSet<string> AllowedRoles =
  [
    DefaultRoles.Admin,
  ];

  public UpdateRolesValidator(IStringLocalizer<SharedResource> localizer)
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleForEach(x => x.Roles)
      .Must(role => AllowedRoles.Contains(role))
      .WithMessage(role => string.Format(localizer[SharedResource.Keys.InvalidAssignableRole], role))
      .OverridePropertyName("roles");
  }
}
