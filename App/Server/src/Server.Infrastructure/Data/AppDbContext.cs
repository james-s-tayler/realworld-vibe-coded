using Server.Core.ArticleAggregate;
using Server.Core.UserAggregate;
using Server.SharedKernel.Interfaces;

namespace Server.Infrastructure.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options,
  IDomainEventDispatcher? dispatcher,
  ITimeProvider timeProvider) : DbContext(options)
{
  private readonly IDomainEventDispatcher? _dispatcher = dispatcher;
  private readonly ITimeProvider _timeProvider = timeProvider;

  public DbSet<User> Users => Set<User>();
  public DbSet<Article> Articles => Set<Article>();
  public DbSet<Tag> Tags => Set<Tag>();
  public DbSet<Comment> Comments => Set<Comment>();
  public DbSet<UserFollowing> UserFollowings => Set<UserFollowing>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    // Configure properties for all entities inheriting from EntityBase
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
      // Check if the entity inherits from any of the EntityBase variants
      if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType) ||
          (entityType.ClrType.BaseType?.IsGenericType == true &&
           entityType.ClrType.BaseType.GetGenericTypeDefinition().Name.StartsWith(nameof(EntityBase))))
      {
        // Configure ChangeCheck for optimistic concurrency
        var changeCheckProperty = entityType.FindProperty(nameof(EntityBase.ChangeCheck));
        if (changeCheckProperty != null)
        {
          modelBuilder.Entity(entityType.ClrType)
            .Property(nameof(EntityBase.ChangeCheck))
            .IsRowVersion();
        }

        // Configure audit timestamps
        var createdAtProperty = entityType.FindProperty(nameof(EntityBase.CreatedAt));
        if (createdAtProperty != null)
        {
          modelBuilder.Entity(entityType.ClrType)
            .Property(nameof(EntityBase.CreatedAt))
            .IsRequired();
        }

        var updatedAtProperty = entityType.FindProperty(nameof(EntityBase.UpdatedAt));
        if (updatedAtProperty != null)
        {
          modelBuilder.Entity(entityType.ClrType)
            .Property(nameof(EntityBase.UpdatedAt))
            .IsRequired();
        }
      }
    }
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
  {
    // Set audit timestamps for entities
    var currentTime = _timeProvider.UtcNow;
    var entries = ChangeTracker.Entries<EntityBase>();
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
          .Where(p => p.Metadata.Name != nameof(EntityBase.UpdatedAt)
                      && p.Metadata.Name != nameof(EntityBase.ChangeCheck)
                      && p.Metadata.Name != nameof(EntityBase.CreatedAt))
          .Any(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue));

        if (hasActualChanges)
        {
          entry.Entity.UpdatedAt = currentTime;
        }
        else
        {
          // If no actual properties changed, don't update UpdatedAt
          // and mark it as not modified to prevent unnecessary updates
          entry.Property(nameof(EntityBase.UpdatedAt)).IsModified = false;
        }
      }
    }

    int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    // ignore events if no dispatcher provided
    if (_dispatcher == null)
    {
      return result;
    }

    // dispatch events only if save was successful
    var entitiesWithEvents = ChangeTracker.Entries<HasDomainEventsBase>()
        .Select(e => e.Entity)
        .Where(e => e.DomainEvents.Any())
        .ToArray();

    await _dispatcher.DispatchAndClearEvents(entitiesWithEvents);

    return result;
  }

  public override int SaveChanges() =>
        SaveChangesAsync().GetAwaiter().GetResult();
}
