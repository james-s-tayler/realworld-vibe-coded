using Server.Core.IdentityAggregate;

namespace Server.Web.Users.List;

/// <summary>
/// Mapper for ApplicationUser to UserDto
/// </summary>
public static class UserMapper
{
  public static UserDto ToDto(ApplicationUser user)
  {
    return new UserDto
    {
      Email = user.Email!,
      Username = user.UserName!,
      Bio = user.Bio,
      Image = user.Image,
    };
  }

  public static List<UserDto> ToDto(List<ApplicationUser> users)
  {
    return users.Select(ToDto).ToList();
  }
}
