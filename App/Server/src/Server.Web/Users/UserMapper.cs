using Server.UseCases.Users;

namespace Server.Web.Users;

/// <summary>
/// FastEndpoints mapper for UserDto to UserResponse
/// Maps UserDto from use cases to response DTO for endpoints
/// </summary>
public class UserMapper : ResponseMapper<UserResponse, UserDto>
{
  public override UserResponse FromEntity(UserDto userDto)
  {
    return new UserResponse
    {
      Email = userDto.Email,
      Username = userDto.Username,
      Bio = userDto.Bio,
      Image = userDto.Image,
      Token = userDto.Token
    };
  }
}
