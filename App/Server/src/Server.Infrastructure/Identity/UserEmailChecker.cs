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
    var userId = entry.Property<Guid>("Id").CurrentValue;

    // PV003: Raw SQL is necessary here because ExecuteUpdateAsync expressions cannot use captured
    // variables or complex expressions that EF Core can't translate. Since we're working with a user
    // entity loaded via IgnoreQueryFilters() (required for ClaimsStrategy), we cannot use UserManager
    // methods which apply tenant filters. Direct SQL update avoids entity tracking conflicts.
#pragma warning disable PV003
    await _dbContext.Database.ExecuteSqlRawAsync(
      "UPDATE AspNetUsers SET AccessFailedCount = AccessFailedCount + 1 WHERE Id = {0}",
      cancellationToken,
      userId);
#pragma warning restore PV003
  }

  public async Task ResetAccessFailedCountAsync<TUser>(TUser user, CancellationToken cancellationToken = default) where TUser : class
  {
    var entry = _dbContext.Entry(user);
    var userId = entry.Property<Guid>("Id").CurrentValue;

    // PV003: Raw SQL is necessary here for same reasons as IncrementAccessFailedCountAsync.
    // We need to update the database directly without entity tracking conflicts.
#pragma warning disable PV003
    await _dbContext.Database.ExecuteSqlRawAsync(
      "UPDATE AspNetUsers SET AccessFailedCount = 0 WHERE Id = {0}",
      cancellationToken,
      userId);
#pragma warning restore PV003
  }
}
