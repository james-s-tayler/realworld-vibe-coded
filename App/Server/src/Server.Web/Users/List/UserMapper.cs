namespace Server.Web.Users.List;

/// <summary>
/// FastEndpoints mapper for List of UserWithRoles to UsersResponse DTO
/// Maps list of users with their roles to response DTO with user information
/// </summary>
public class UserMapper : ResponseMapper<UsersResponse, List<Server.UseCases.Users.List.UserWithRoles>>
{
  public override Task<UsersResponse> FromEntityAsync(List<Server.UseCases.Users.List.UserWithRoles> usersWithRoles, CancellationToken ct)
  {
    var userDtos = usersWithRoles.Select(uwr => new UserDto
    {
      Email = uwr.User.Email!,
      Username = uwr.User.UserName!,
      Bio = uwr.User.Bio,
      Image = uwr.User.Image,
      Roles = uwr.Roles,
    }).ToList();

    return Task.FromResult(new UsersResponse { Users = userDtos });
  }
}
