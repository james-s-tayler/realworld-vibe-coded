using Server.UseCases.Users.List;

namespace Server.Web.Users.List;

public class UserMapper : ResponseMapper<UsersResponse, ListUsersResult>
{
  public override Task<UsersResponse> FromEntityAsync(ListUsersResult result, CancellationToken ct)
  {
    var userDtos = result.Users.Select(user => new UserDto
    {
      Id = user.Id,
      Email = user.Email,
      Username = user.Username,
      Bio = user.Bio,
      Image = user.Image,
      Roles = user.Roles,
      IsActive = user.IsActive,
    }).ToList();

    return Task.FromResult(new UsersResponse { Users = userDtos, UsersCount = result.TotalCount });
  }
}
