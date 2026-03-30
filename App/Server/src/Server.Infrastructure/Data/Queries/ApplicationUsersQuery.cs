using Server.UseCases.Interfaces;
using Server.UseCases.Users.Dtos;

namespace Server.Infrastructure.Data.Queries;

public class ApplicationUsersQuery : IQueryApplicationUsers
{
  private readonly AppDbContext _dbContext;

  public ApplicationUsersQuery(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<UserWithRolesDto?> GetCurrentUserWithRoles(Guid userId, CancellationToken cancellationToken = default)
  {
    var userWithRoles = await (
      from user in _dbContext.Users.AsNoTracking()
      where user.Id == userId
      select new
      {
        user.Id,
        user.Email,
        user.UserName,
        user.Bio,
        user.Image,
        user.LockoutEnd,
        Roles = (from ur in _dbContext.UserRoles
                 where ur.UserId == user.Id
                 join r in _dbContext.Roles on ur.RoleId equals r.Id
                 select r.Name!).AsEnumerable(),
      })
      .FirstOrDefaultAsync(cancellationToken);

    if (userWithRoles == null)
    {
      return null;
    }

    return new UserWithRolesDto(
      userWithRoles.Id,
      userWithRoles.Email!,
      userWithRoles.UserName!,
      userWithRoles.Bio ?? string.Empty,
      userWithRoles.Image,
      userWithRoles.Roles.ToList(),
      !(userWithRoles.LockoutEnd.HasValue && userWithRoles.LockoutEnd.Value > DateTimeOffset.UtcNow));
  }

  public async Task<List<UserWithRolesDto>> ListUsersWithRoles(int limit, int offset, CancellationToken cancellationToken = default)
  {
    var usersWithRoles = await (
      from user in _dbContext.Users.AsNoTracking()
      orderby user.UserName
      select new
      {
        user.Id,
        user.Email,
        user.UserName,
        user.Bio,
        user.Image,
        user.LockoutEnd,
        Roles = (from ur in _dbContext.UserRoles
                 where ur.UserId == user.Id
                 join r in _dbContext.Roles on ur.RoleId equals r.Id
                 select r.Name!).AsEnumerable(),
      })
      .Skip(offset)
      .Take(limit)
      .ToListAsync(cancellationToken);

    return usersWithRoles.Select(u => new UserWithRolesDto(
      u.Id,
      u.Email!,
      u.UserName!,
      u.Bio ?? string.Empty,
      u.Image,
      u.Roles.ToList(),
      !(u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow))).ToList();
  }

  public async Task<int> CountUsers(CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users.AsNoTracking().CountAsync(cancellationToken);
  }
}
