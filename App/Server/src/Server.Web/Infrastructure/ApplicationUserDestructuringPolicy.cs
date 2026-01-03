using Microsoft.AspNetCore.Identity;
using Serilog.Core;
using Serilog.Events;
using Server.Core.IdentityAggregate;

namespace Server.Web.Infrastructure;

public class ApplicationUserDestructuringPolicy : IDestructuringPolicy
{
  private const string SensitiveDataMask = "***REDACTED***";

  public bool TryDestructure(
    object value,
    ILogEventPropertyValueFactory propertyValueFactory,
    out LogEventPropertyValue result)
  {
    if (value is ApplicationUser user)
    {
      var safeProperties = new Dictionary<string, object?>
      {
        { nameof(user.Id), user.Id },
        { nameof(user.UserName), user.UserName },
        { nameof(user.Email), user.Email },
        { nameof(user.Bio), user.Bio },
        { nameof(user.Image), user.Image },
        { nameof(user.EmailConfirmed), user.EmailConfirmed },
        { nameof(user.PhoneNumberConfirmed), user.PhoneNumberConfirmed },
        { nameof(user.TwoFactorEnabled), user.TwoFactorEnabled },
        { nameof(user.LockoutEnabled), user.LockoutEnabled },
        { nameof(user.LockoutEnd), user.LockoutEnd },
        { nameof(user.AccessFailedCount), user.AccessFailedCount },
        { nameof(IdentityUser.PasswordHash), SensitiveDataMask },
        { nameof(IdentityUser.SecurityStamp), SensitiveDataMask },
        { nameof(IdentityUser.ConcurrencyStamp), SensitiveDataMask },
      };

      result = propertyValueFactory.CreatePropertyValue(safeProperties, destructureObjects: true);
      return true;
    }

    result = null!;
    return false;
  }
}
