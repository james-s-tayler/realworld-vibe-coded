using Server.Infrastructure.Data;
using Server.SharedKernel.Identity;

namespace Server.Infrastructure.Identity;

/// <summary>
/// Implementation of IUserEmailChecker that bypasses multi-tenant query filters.
/// </summary>
public class UserEmailChecker : IUserEmailChecker
{
  private readonly AppDbContext _dbContext;

  public UserEmailChecker(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
  {
    var normalizedEmail = email.ToUpperInvariant();
    return await _dbContext.Users
      .IgnoreQueryFilters()
      .AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
  }

  public async Task<TUser?> FindByEmailAsync<TUser>(string email, CancellationToken cancellationToken = default) where TUser : class
  {
    var normalizedEmail = email.ToUpperInvariant();
    return await _dbContext.Set<TUser>()
      .IgnoreQueryFilters()
      .FirstOrDefaultAsync(u => EF.Property<string>(u, "NormalizedEmail") == normalizedEmail, cancellationToken);
  }
}
