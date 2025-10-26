using Audit.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Server.Core.ArticleAggregate;
using Server.Core.UserAggregate;

namespace Server.Infrastructure.Data;

[AuditDbContext(Mode = AuditOptionMode.OptOut, IncludeEntityObjects = false)]
public class AppDbContext : AuditIdentityDbContext<User, IdentityRole<Guid>, Guid>
{
  private readonly IDomainEventDispatcher? _dispatcher;

  public AppDbContext(DbContextOptions<AppDbContext> options,
    IDomainEventDispatcher? dispatcher) : base(options)
  {
    _dispatcher = dispatcher;
  }

  public new DbSet<User> Users => Set<User>();
  public DbSet<Article> Articles => Set<Article>();
  public DbSet<Tag> Tags => Set<Tag>();
  public DbSet<Comment> Comments => Set<Comment>();
  public DbSet<UserFollowing> UserFollowings => Set<UserFollowing>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    // Configure Identity table names to avoid conflicts
    modelBuilder.Entity<User>().ToTable("Users");
    modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
    modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
    modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
    modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
    modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
    modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

    // Configure properties for all entities inheriting from EntityBase
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
      // Skip Identity entities
      if (entityType.ClrType.Namespace?.StartsWith("Microsoft.AspNetCore.Identity") == true)
      {
        continue;
      }

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

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
  {
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
