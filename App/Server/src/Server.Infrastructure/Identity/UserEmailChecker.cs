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
      .AsNoTracking()
      .AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
  }

  public async Task<TUser?> FindByEmailAsync<TUser>(string email, CancellationToken cancellationToken = default) where TUser : class
  {
    var normalizedEmail = email.ToUpperInvariant();
    return await _dbContext.Set<TUser>()
      .IgnoreQueryFilters()
      .AsNoTracking()
      .FirstOrDefaultAsync(u => EF.Property<string>(u, "NormalizedEmail") == normalizedEmail, cancellationToken);
  }

  public string GetTenantId<TUser>(TUser user) where TUser : class
  {
    // For AsNoTracking queries, the entity may not be in the context's change tracker
    // Try to get the entry first, and if it's detached, attach temporarily to read the shadow property
    var entry = _dbContext.Entry(user);

    // If the entity state is Detached, we need to attach it temporarily to access shadow properties
    if (entry.State == EntityState.Detached)
    {
      // Attach without tracking to read shadow properties
      _dbContext.Attach(user);
      var tenantId = entry.Property<string>("TenantId").CurrentValue;

      // Detach immediately after reading
      entry.State = EntityState.Detached;
      return tenantId;
    }

    return entry.Property<string>("TenantId").CurrentValue;
  }
}
