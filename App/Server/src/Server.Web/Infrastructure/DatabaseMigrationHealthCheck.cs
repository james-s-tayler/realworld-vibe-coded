using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Server.Infrastructure.Data;

namespace Server.Web.Infrastructure;

public class DatabaseMigrationHealthCheck : IHealthCheck
{
  private static readonly string _migrationCompleteCacheKey = $"{nameof(DatabaseMigrationHealthCheck)}-isMigrationComplete";
  private readonly AppDbContext _db;
  private readonly IMemoryCache _cache;

  public DatabaseMigrationHealthCheck(AppDbContext db, IMemoryCache cache)
  {
    _db = db;
    _cache = cache;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(
    HealthCheckContext context,
    CancellationToken cancellationToken = default)
  {
    // 1) Can we even connect?
    if (!await _db.Database.CanConnectAsync(cancellationToken))
    {
      return HealthCheckResult.Unhealthy("Cannot connect to database.");
    }

    var isMigrationComplete = _cache.GetOrCreate(_migrationCompleteCacheKey, _ => false);

    // 2) Are there any pending migrations?
    if (!isMigrationComplete)
    {
      var pending = await _db.Database.GetPendingMigrationsAsync(cancellationToken);
      var pendingList = pending.ToList();

      if (pendingList.Any())
      {
        return HealthCheckResult.Unhealthy(
          $"Database has pending migrations: {string.Join(", ", pendingList)}");
      }

      _cache.Set(_migrationCompleteCacheKey, true);
    }

    return HealthCheckResult.Healthy("Database schema is up to date.");
  }
}
