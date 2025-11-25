using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;

namespace Server.Web.Infrastructure;

/// <summary>
/// Default implementation using Entity Framework's DbContext.
/// </summary>
public class EfDatabaseMigrationChecker : IDatabaseMigrationChecker
{
  private readonly AppDbContext _db;

  public EfDatabaseMigrationChecker(AppDbContext db)
  {
    _db = db;
  }

  public Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    => _db.Database.CanConnectAsync(cancellationToken);

  public Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken)
    => _db.Database.GetPendingMigrationsAsync(cancellationToken);
}
