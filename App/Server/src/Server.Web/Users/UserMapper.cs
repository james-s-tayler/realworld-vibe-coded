using Server.Core.IdentityAggregate;

namespace Server.Web.Users;

/// <summary>
/// FastEndpoints mapper for ApplicationUser entity to UserResponse
/// Maps domain entity to response DTO
/// </summary>
public class UserMapper : ResponseMapper<UserResponse, ApplicationUser>
{
  public override UserResponse FromEntity(ApplicationUser user)
  {
    return new UserResponse
    {
      Email = user.Email!,
      Username = user.UserName!,
      Bio = user.Bio,
      Image = user.Image,
      Token = string.Empty, // Token not used with cookie authentication
    };
  }
}
