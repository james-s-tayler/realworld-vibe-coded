using Server.UseCases.Users.Dtos;

namespace Server.Web.Users.List;

public class UserMapper : ResponseMapper<UsersResponse, List<UserWithRolesDto>>
{
  public override Task<UsersResponse> FromEntityAsync(List<UserWithRolesDto> users, CancellationToken ct)
  {
    var userDtos = users.Select(user => new UserDto
    {
      Email = user.Email,
      Username = user.Username,
      Bio = user.Bio,
      Image = user.Image,
      Roles = user.Roles,
    }).ToList();

    return Task.FromResult(new UsersResponse { Users = userDtos });
  }
}
