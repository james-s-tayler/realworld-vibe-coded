using Server.Core.UserAggregate;
using Server.UseCases.Interfaces;

namespace Server.Web.Users;

/// <summary>
/// FastEndpoints mapper for User entity to UserResponse
/// Maps domain entity to response DTO with JWT token generation
/// </summary>
public class UserMapper : ResponseMapper<UserResponse, User>
{
  public override UserResponse FromEntity(User user)
  {
    // Check if there's already a valid token in the current request
    var currentUserService = Resolve<IUserContext>();
    var existingToken = currentUserService.GetCurrentToken();

    // Use existing token if present and user is authenticated, otherwise generate new one
    string token;
    if (!string.IsNullOrEmpty(existingToken) && currentUserService.IsAuthenticated())
    {
      token = existingToken;
    }
    else
    {
      // Generate a new token for Login/Register or when no valid token exists
      var jwtTokenGenerator = Resolve<IJwtTokenGenerator>();
      token = jwtTokenGenerator.GenerateToken(user);
    }

    return new UserResponse
    {
      Email = user.Email,
      Username = user.Username,
      Bio = user.Bio,
      Image = user.Image,
      Token = token,
    };
  }
}
