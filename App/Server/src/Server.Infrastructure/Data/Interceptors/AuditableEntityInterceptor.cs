using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Server.Core.Interfaces;
using Server.SharedKernel.Interfaces;

namespace Server.Infrastructure.Data.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets audit timestamps and user tracking on IAuditableEntity instances.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
  private readonly ITimeProvider _timeProvider;
  private readonly IServiceProvider _serviceProvider;

  public AuditableEntityInterceptor(ITimeProvider timeProvider, IServiceProvider serviceProvider)
  {
    _timeProvider = timeProvider;
    _serviceProvider = serviceProvider;
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

    // Get the current user service from the scoped service provider via the DbContext
    // This may be null if called outside of an HTTP request context (e.g., during migrations or tests)
    IUserContext? currentUserService = null;
    try
    {
      // Try to get the scoped service provider from the context's service provider
      // The context's service provider is scoped to the current request
      var scopedProvider = context.GetInfrastructure();
      currentUserService = scopedProvider.GetService<IUserContext>();
    }
    catch
    {
      // Ignore errors - we'll fall back to "SYSTEM"
    }

    var currentUsername = currentUserService?.GetCurrentUsername() ?? "SYSTEM";

    var entries = context.ChangeTracker.Entries<IAuditableEntity>();

    foreach (var entry in entries)
    {
      if (entry.State == EntityState.Added)
      {
        entry.Entity.CreatedAt = currentTime;
        entry.Entity.UpdatedAt = currentTime;
        entry.Entity.CreatedBy = currentUsername;
        entry.Entity.UpdatedBy = currentUsername;
      }
      else if (entry.State == EntityState.Modified)
      {
        // Only update UpdatedAt/UpdatedBy if actual scalar properties have changed values
        // We check if any property's original value differs from its current value
        // Exclude: UpdatedAt, UpdatedBy, ChangeCheck (concurrency token), CreatedAt, and CreatedBy
        var hasActualChanges = entry.Properties
          .Where(p => p.Metadata.Name != nameof(IAuditableEntity.UpdatedAt)
                      && p.Metadata.Name != nameof(IAuditableEntity.UpdatedBy)
                      && p.Metadata.Name != nameof(IAuditableEntity.ChangeCheck)
                      && p.Metadata.Name != nameof(IAuditableEntity.CreatedAt)
                      && p.Metadata.Name != nameof(IAuditableEntity.CreatedBy))
          .Any(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue));

        if (hasActualChanges)
        {
          entry.Entity.UpdatedAt = currentTime;
          entry.Entity.UpdatedBy = currentUsername;
        }
        else
        {
          // If no actual properties changed, don't update UpdatedAt/UpdatedBy
          // and mark them as not modified to prevent unnecessary updates
          entry.Property(nameof(IAuditableEntity.UpdatedAt)).IsModified = false;
          entry.Property(nameof(IAuditableEntity.UpdatedBy)).IsModified = false;
        }
      }
    }
  }
}
