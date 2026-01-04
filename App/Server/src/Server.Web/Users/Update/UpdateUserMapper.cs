using Server.Core.IdentityAggregate;

namespace Server.Web.Users.Update;

/// <summary>
/// FastEndpoints mapper for ApplicationUser to UpdateUserResponse DTO
/// Maps the updated user to response DTO
/// </summary>
public class UpdateUserMapper : ResponseMapper<UpdateUserResponse, ApplicationUser>
{
  public override Task<UpdateUserResponse> FromEntityAsync(ApplicationUser user, CancellationToken ct)
  {
    var response = new UpdateUserResponse
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
