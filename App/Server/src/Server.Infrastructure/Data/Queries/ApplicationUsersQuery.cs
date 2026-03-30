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
        user.Language,
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
      !(userWithRoles.LockoutEnd.HasValue && userWithRoles.LockoutEnd.Value > DateTimeOffset.UtcNow),
      userWithRoles.Language);
  }

  public async Task<List<UserWithRolesDto>> ListUsersWithRoles(int limit, int offset, CancellationToken cancellationToken = default)
  {
    var users = await _dbContext.Users.AsNoTracking()
      .OrderBy(u => u.UserName)
      .Skip(offset)
      .Take(limit)
      .Select(u => new { u.Id, u.Email, u.UserName, u.Bio, u.Image, u.LockoutEnd, u.Language })
      .ToListAsync(cancellationToken);

    var userIds = users.Select(u => u.Id).ToList();

    var userRoles = await (
      from ur in _dbContext.UserRoles.AsNoTracking()
      where userIds.Contains(ur.UserId)
      join r in _dbContext.Roles on ur.RoleId equals r.Id
      select new { ur.UserId, RoleName = r.Name! })
      .ToListAsync(cancellationToken);

    var rolesByUser = userRoles
      .GroupBy(x => x.UserId)
      .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

    return users.Select(u => new UserWithRolesDto(
      u.Id,
      u.Email!,
      u.UserName!,
      u.Bio ?? string.Empty,
      u.Image,
      rolesByUser.GetValueOrDefault(u.Id, []),
      !(u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow),
      u.Language)).ToList();
  }

  public async Task<int> CountUsers(CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users.AsNoTracking().CountAsync(cancellationToken);
  }
}
