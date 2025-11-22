using Microsoft.Extensions.Diagnostics.HealthChecks;
using Server.Infrastructure.Data;

namespace Server.Web.Infrastructure;

public class DatabaseMigrationHealthCheck : IHealthCheck
{
  private readonly AppDbContext _db;

  public DatabaseMigrationHealthCheck(AppDbContext db)
  {
    _db = db;
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

    // 2) Are there any pending migrations?
    var pending = await _db.Database.GetPendingMigrationsAsync(cancellationToken);
    var pendingList = pending.ToList();

    if (pendingList.Any())
    {
      return HealthCheckResult.Unhealthy(
        $"Database has pending migrations: {string.Join(", ", pendingList)}");
    }

    return HealthCheckResult.Healthy("Database schema is up to date.");
  }
}
