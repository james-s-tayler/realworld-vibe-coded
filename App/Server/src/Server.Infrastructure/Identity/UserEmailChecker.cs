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

  public string GetTenantId<TUser>(TUser user) where TUser : class
  {
    var entry = _dbContext.Entry(user);
    return entry.Property<string>("TenantId").CurrentValue;
  }

  public async Task IncrementAccessFailedCountAsync<TUser>(TUser user, CancellationToken cancellationToken = default) where TUser : class
  {
    var entry = _dbContext.Entry(user);

    // Get current values
    var userId = entry.Property<Guid>("Id").CurrentValue;
    var currentCount = entry.Property<int>("AccessFailedCount").CurrentValue;
    var newCount = currentCount + 1;

    // Use ExecuteUpdateAsync to update directly in the database without tracking issues
    await _dbContext.Set<TUser>()
      .Where(u => EF.Property<Guid>(u, "Id") == userId)
      .ExecuteUpdateAsync(
        setters => setters
          .SetProperty(u => EF.Property<int>(u, "AccessFailedCount"), newCount),
        cancellationToken);
  }

  public async Task ResetAccessFailedCountAsync<TUser>(TUser user, CancellationToken cancellationToken = default) where TUser : class
  {
    var entry = _dbContext.Entry(user);

    // Use ExecuteUpdateAsync to update directly in the database without tracking issues
    // ApplicationUser.Id is Guid, not string
    var userId = entry.Property<Guid>("Id").CurrentValue;
    await _dbContext.Set<TUser>()
      .Where(u => EF.Property<Guid>(u, "Id") == userId)
      .ExecuteUpdateAsync(
        setters => setters
          .SetProperty(u => EF.Property<int>(u, "AccessFailedCount"), 0),
        cancellationToken);
  }
}
