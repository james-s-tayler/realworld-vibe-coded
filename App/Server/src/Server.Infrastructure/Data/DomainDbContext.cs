using Audit.EntityFramework;
using Server.Core.ArticleAggregate;
using Server.Core.UserAggregate;

namespace Server.Infrastructure.Data;

[AuditDbContext(Mode = AuditOptionMode.OptOut, IncludeEntityObjects = false)]
public class DomainDbContext : AuditDbContext
{
  private readonly IDomainEventDispatcher? _dispatcher;

  public DomainDbContext(DbContextOptions<DomainDbContext> options,
    IDomainEventDispatcher? dispatcher) : base(options)
  {
    _dispatcher = dispatcher;
  }

  public DbSet<Article> Articles => Set<Article>();
  public DbSet<Tag> Tags => Set<Tag>();
  public DbSet<Comment> Comments => Set<Comment>();
  public DbSet<UserFollowing> UserFollowings => Set<UserFollowing>();

  // User is read-only in DomainDbContext - modifications happen in IdentityDbContext
  // This allows querying User information for relationships and foreign keys
  public DbSet<User> Users => Set<User>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    // Configure User as query-only (no table, just for relationships)
    // The actual User table is in IdentityDbContext
    modelBuilder.Entity<User>()
      .ToTable("Users") // Same table name, but this context won't create/modify it
      .HasKey(u => u.Id);

    // Configure only the properties needed for domain relationships
    modelBuilder.Entity<User>()
      .Property(u => u.Email)
      .HasMaxLength(256);

    modelBuilder.Entity<User>()
      .Property(u => u.UserName)
      .HasMaxLength(256);

    modelBuilder.Entity<User>()
      .Property(u => u.NormalizedUserName)
      .HasMaxLength(256);

    // Configure UserFollowing relationships properly
    modelBuilder.Entity<UserFollowing>()
      .HasOne(uf => uf.Follower)
      .WithMany(u => u.Following)
      .HasForeignKey(uf => uf.FollowerId)
      .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<UserFollowing>()
      .HasOne(uf => uf.Followed)
      .WithMany(u => u.Followers)
      .HasForeignKey(uf => uf.FollowedId)
      .OnDelete(DeleteBehavior.Restrict);

    // Configure properties for all entities inheriting from EntityBase
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
      // Skip User entity - it's configured in IdentityDbContext
      if (entityType.ClrType == typeof(User))
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
