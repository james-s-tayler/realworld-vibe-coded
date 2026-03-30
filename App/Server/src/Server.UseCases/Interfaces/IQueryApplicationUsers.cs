using Server.UseCases.Users.Dtos;

namespace Server.UseCases.Interfaces;

public interface IQueryApplicationUsers
{
  Task<UserWithRolesDto?> GetCurrentUserWithRoles(Guid userId, CancellationToken cancellationToken = default);

  Task<List<UserWithRolesDto>> ListUsersWithRoles(int limit, int offset, CancellationToken cancellationToken = default);

  Task<int> CountUsers(CancellationToken cancellationToken = default);
}
