using Server.UseCases.Users.Dtos;

namespace Server.Web.Users.Update;

public class UpdateUserMapper : ResponseMapper<UpdateUserResponse, UserWithRolesDto>
{
  public override Task<UpdateUserResponse> FromEntityAsync(UserWithRolesDto user, CancellationToken ct)
  {
    var response = new UpdateUserResponse
    {
      User = new UserResponse
      {
        Email = user.Email,
        Username = user.Username,
        Bio = user.Bio,
        Image = user.Image,
        Roles = user.Roles,
        Language = user.Language,
      },
    };

    return Task.FromResult(response);
  }
}
