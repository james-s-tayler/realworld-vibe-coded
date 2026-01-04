using Server.UseCases.Users.Dtos;

namespace Server.Web.Users.GetCurrent;

/// <summary>
/// FastEndpoints mapper for ApplicationUser to UserCurrentResponse DTO
/// Maps the current authenticated user to response DTO
/// </summary>
public class UserCurrentMapper : ResponseMapper<UserCurrentResponse, UserWithRolesDto>
{
  public override Task<UserCurrentResponse> FromEntityAsync(UserWithRolesDto user, CancellationToken ct)
  {
    var response = new UserCurrentResponse
    {
      User = new UserResponse
      {
        Email = user.Email,
        Username = user.Username,
        Bio = user.Bio,
        Image = user.Image,
        Roles = user.Roles,
      },
    };

    return Task.FromResult(response);
  }
}
