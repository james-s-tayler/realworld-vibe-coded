using Server.Core.UserAggregate;

namespace Server.Infrastructure.Data.Config;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    // Bio and Image are custom fields
    builder.Property(p => p.Bio)
        .HasMaxLength(DataSchemaConstants.BIO_LENGTH)
        .IsRequired();

    builder.Property(p => p.Image)
        .HasMaxLength(DataSchemaConstants.IMAGE_URL_LENGTH);

    // Configure audit fields
    builder.Property(p => p.CreatedAt)
        .IsRequired();

    builder.Property(p => p.UpdatedAt)
        .IsRequired();

    builder.Property(p => p.CreatedBy)
        .IsRequired()
        .HasMaxLength(256);

    builder.Property(p => p.UpdatedBy)
        .IsRequired()
        .HasMaxLength(256);

    builder.Property(p => p.ChangeCheck)
        .IsRowVersion();

    // Explicitly configure navigation properties
    // These are configured in UserFollowingConfiguration, but we need to ensure
    // they're properly linked from the User side as well
    builder.HasMany(u => u.Following)
        .WithOne(uf => uf.Follower)
        .HasForeignKey(uf => uf.FollowerId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasMany(u => u.Followers)
        .WithOne(uf => uf.Followed)
        .HasForeignKey(uf => uf.FollowedId)
        .OnDelete(DeleteBehavior.Restrict);

    // Note: Email, UserName, and other Identity fields are configured by IdentityDbContext
    // We only ensure our custom unique indexes match the database schema expectations
    builder.HasIndex(p => p.NormalizedEmail)
        .IsUnique()
        .HasDatabaseName("IX_Users_Email");

    builder.HasIndex(p => p.NormalizedUserName)
        .IsUnique()
        .HasDatabaseName("IX_Users_Username");
  }
}
