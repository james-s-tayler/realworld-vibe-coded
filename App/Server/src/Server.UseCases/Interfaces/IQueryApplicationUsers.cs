using Server.UseCases.Users.Dtos;

namespace Server.UseCases.Interfaces;

public interface IQueryApplicationUsers
{
  Task<UserWithRolesDto?> GetCurrentUserWithRoles(Guid userId, CancellationToken cancellationToken = default);

  Task<List<UserWithRolesDto>> ListUsersWithRoles(CancellationToken cancellationToken = default);
}
