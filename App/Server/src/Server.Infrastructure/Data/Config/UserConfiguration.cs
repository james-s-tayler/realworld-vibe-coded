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

    // Note: Navigation properties (Following/Followers) are configured in UserFollowingConfiguration
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
