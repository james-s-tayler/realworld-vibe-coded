using Audit.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Server.Core.UserAggregate;

namespace Server.Infrastructure.Data;

[AuditDbContext(Mode = AuditOptionMode.OptOut, IncludeEntityObjects = false)]
public class IdentityDbContext : AuditIdentityDbContext<User, IdentityRole<Guid>, Guid>
{
  public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Apply User configuration
    modelBuilder.ApplyConfiguration(new Config.UserConfiguration());

    // Configure Identity table names
    modelBuilder.Entity<User>().ToTable("Users");
    modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
    modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
    modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
    modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
    modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
    modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

    // Configure audit fields for User entity
    modelBuilder.Entity<User>()
      .Property(u => u.CreatedAt)
      .IsRequired();

    modelBuilder.Entity<User>()
      .Property(u => u.UpdatedAt)
      .IsRequired();

    modelBuilder.Entity<User>()
      .Property(u => u.CreatedBy)
      .IsRequired()
      .HasMaxLength(256);

    modelBuilder.Entity<User>()
      .Property(u => u.UpdatedBy)
      .IsRequired()
      .HasMaxLength(256);

    modelBuilder.Entity<User>()
      .Property(u => u.ChangeCheck)
      .IsRowVersion();

    // Exclude UserFollowing relationships from Identity context
    // These will be managed by DomainDbContext
    modelBuilder.Ignore<UserFollowing>();
  }
}
