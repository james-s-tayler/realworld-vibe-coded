namespace Server.Web.Infrastructure;

/// <summary>
/// Interface for database migration checking, allowing for testable health checks.
/// </summary>
public interface IDatabaseMigrationChecker
{
  Task<bool> CanConnectAsync(CancellationToken cancellationToken);

  Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken);
}
