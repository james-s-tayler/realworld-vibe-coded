using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Web.Users.Register;

namespace Server.Web.Users;

/// <summary>
/// FastEndpoints mapper for User entity to UserResponse
/// Maps domain entity to response DTO with JWT token generation
/// </summary>
public class UserMapper : ResponseMapper<UserResponse, User>
{
  public override UserResponse FromEntity(User user)
  {
    // Resolve JWT token generator to create token
    var jwtTokenGenerator = Resolve<IJwtTokenGenerator>();
    var token = jwtTokenGenerator.GenerateToken(user);

    return new UserResponse
    {
      Email = user.Email,
      Username = user.Username,
      Bio = user.Bio,
      Image = user.Image,
      Token = token
    };
  }
}
