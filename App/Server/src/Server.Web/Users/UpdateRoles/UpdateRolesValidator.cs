using FluentValidation;
using Server.Core.IdentityAggregate;

namespace Server.Web.Users.UpdateRoles;

public class UpdateRolesValidator : Validator<UpdateRolesRequest>
{
  private static readonly HashSet<string> AllowedRoles =
  [
    DefaultRoles.Admin,
    DefaultRoles.Author,
    DefaultRoles.Moderator,
  ];

  public UpdateRolesValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Roles)
      .NotEmpty().WithMessage("At least one role is required.")
      .OverridePropertyName("roles");

    RuleForEach(x => x.Roles)
      .Must(role => AllowedRoles.Contains(role))
      .WithMessage(role => $"'{role}' is not a valid assignable role. Allowed roles: {string.Join(", ", AllowedRoles)}")
      .OverridePropertyName("roles");
  }
}
