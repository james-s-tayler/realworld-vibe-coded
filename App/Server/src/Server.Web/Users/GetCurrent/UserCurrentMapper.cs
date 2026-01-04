using Server.Core.IdentityAggregate;

namespace Server.Web.Users.GetCurrent;

/// <summary>
/// FastEndpoints mapper for ApplicationUser to UserCurrentResponse DTO
/// Maps the current authenticated user to response DTO
/// </summary>
public class UserCurrentMapper : ResponseMapper<UserCurrentResponse, ApplicationUser>
{
  public override Task<UserCurrentResponse> FromEntityAsync(ApplicationUser user, CancellationToken ct)
  {
    var response = new UserCurrentResponse
    {
      User = new UserResponse
      {
        Email = user.Email!,
        Username = user.UserName!,
        Bio = user.Bio ?? string.Empty,
        Image = user.Image,
      },
    };

    return Task.FromResult(response);
  }
}
