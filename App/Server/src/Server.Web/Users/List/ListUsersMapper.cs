using Server.UseCases.Users.Dtos;
using Server.Web.Shared.Pagination;

namespace Server.Web.Users.List;

public class ListUsersMapper : PaginatedResponseMapper<UserWithRolesDto, UserDto>
{
  protected override Task<UserDto> MapItemAsync(UserWithRolesDto entity, CancellationToken ct) =>
    Task.FromResult(new UserDto
    {
      Id = entity.Id,
      Email = entity.Email,
      Username = entity.Username,
      Bio = entity.Bio,
      Image = entity.Image,
      Roles = entity.Roles,
      IsActive = entity.IsActive,
      Language = entity.Language,
    });
}
