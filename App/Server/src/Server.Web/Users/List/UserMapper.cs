using Server.Core.IdentityAggregate;

namespace Server.Web.Users.List;

/// <summary>
/// FastEndpoints mapper for List of ApplicationUser to UsersResponse DTO
/// Maps list of users to response DTO with user information
/// </summary>
public class UserMapper : ResponseMapper<UsersResponse, List<ApplicationUser>>
{
  public override Task<UsersResponse> FromEntityAsync(List<ApplicationUser> users, CancellationToken ct)
  {
    var userDtos = users.Select(user => new UserDto
    {
      Email = user.Email!,
      Username = user.UserName!,
      Bio = user.Bio,
      Image = user.Image,
    }).ToList();

    return Task.FromResult(new UsersResponse { Users = userDtos });
  }
}
