using Audit.EntityFramework;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Server.Core.ArticleAggregate;
using Server.Core.IdentityAggregate;
using Server.Core.TagAggregate;
using Server.Core.UserAggregate;
using Server.SharedKernel.DomainEvents;
using Server.SharedKernel.Persistence;

namespace Server.Infrastructure.Data;

[AuditDbContext(Mode = AuditOptionMode.OptOut, IncludeEntityObjects = false)]
public class AppDbContext : MultiTenantIdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
  private readonly IDomainEventDispatcher? _dispatcher;

  public AppDbContext(
    IMultiTenantContextAccessor multiTenantContextAccessor,
    DbContextOptions<AppDbContext> options,
    IDomainEventDispatcher? dispatcher) : base(multiTenantContextAccessor, options)
  {
    _dispatcher = dispatcher;
  }

  public DbSet<Article> Articles => Set<Article>();

  public DbSet<Tag> Tags => Set<Tag>();

  public DbSet<Comment> Comments => Set<Comment>();

  public DbSet<UserFollowing> UserFollowings => Set<UserFollowing>();

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
  {
    // Call base SaveChangesAsync - Audit.NET will intercept via [AuditDbContext] attribute
    var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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

        // Configure audit user tracking
        var createdByProperty = entityType.FindProperty(nameof(EntityBase.CreatedBy));
        if (createdByProperty != null)
        {
          modelBuilder.Entity(entityType.ClrType)
            .Property(nameof(EntityBase.CreatedBy))
            .IsRequired()
            .HasMaxLength(256);
        }

        var updatedByProperty = entityType.FindProperty(nameof(EntityBase.UpdatedBy));
        if (updatedByProperty != null)
        {
          modelBuilder.Entity(entityType.ClrType)
            .Property(nameof(EntityBase.UpdatedBy))
            .IsRequired()
            .HasMaxLength(256);
        }
      }
    }
  }
}
