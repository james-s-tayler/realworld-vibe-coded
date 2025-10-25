using Microsoft.EntityFrameworkCore.Diagnostics;
using Server.SharedKernel.Interfaces;

namespace Server.Infrastructure.Data.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets audit timestamps on IAuditableEntity instances.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
  private readonly ITimeProvider _timeProvider;

  public AuditableEntityInterceptor(ITimeProvider timeProvider)
  {
    _timeProvider = timeProvider;
  }

  public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
  {
    UpdateAuditableEntities(eventData.Context);
    return base.SavingChanges(eventData, result);
  }

  public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
    DbContextEventData eventData,
    InterceptionResult<int> result,
    CancellationToken cancellationToken = default)
  {
    UpdateAuditableEntities(eventData.Context);
    return base.SavingChangesAsync(eventData, result, cancellationToken);
  }

  private void UpdateAuditableEntities(DbContext? context)
  {
    if (context == null)
    {
      return;
    }

    var currentTime = _timeProvider.UtcNow;
    var entries = context.ChangeTracker.Entries<IAuditableEntity>();

    foreach (var entry in entries)
    {
      if (entry.State == EntityState.Added)
      {
        entry.Entity.CreatedAt = currentTime;
        entry.Entity.UpdatedAt = currentTime;
      }
      else if (entry.State == EntityState.Modified)
      {
        // Only update UpdatedAt if actual scalar properties have changed values
        // We check if any property's original value differs from its current value
        // Exclude: UpdatedAt itself, ChangeCheck (concurrency token), and CreatedAt
        var hasActualChanges = entry.Properties
          .Where(p => p.Metadata.Name != nameof(IAuditableEntity.UpdatedAt)
                      && p.Metadata.Name != nameof(IAuditableEntity.ChangeCheck)
                      && p.Metadata.Name != nameof(IAuditableEntity.CreatedAt))
          .Any(p => p.IsModified);

        if (hasActualChanges)
        {
          entry.Entity.UpdatedAt = currentTime;
        }
        else
        {
          // If no actual properties changed, don't update UpdatedAt
          // and mark it as not modified to prevent unnecessary updates
          entry.Property(nameof(IAuditableEntity.UpdatedAt)).IsModified = false;
        }
      }
    }
  }
}
