using Server.Core.UserAggregate;

namespace Server.UseCases.Users;

/// <summary>
/// Static mappers for User-related entities to DTOs to reduce duplication across handlers
/// </summary>
public static class UserMappers
{
  /// <summary>
  /// Maps User entity to UserDto with provided token
  /// </summary>
  public static UserDto MapToDto(User user, string token)
  {
    return new UserDto(
      user.Id,
      user.Email,
      user.Username,
      user.Bio,
      user.Image,
      token +" broken test"
    );
  }
}
