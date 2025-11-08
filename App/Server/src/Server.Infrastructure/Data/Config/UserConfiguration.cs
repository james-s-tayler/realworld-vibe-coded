using Server.Core.UserAggregate;

namespace Server.Infrastructure.Data.Config;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.Property(p => p.Email)
        .HasMaxLength(DataSchemaConstants.EmailLength)
        .IsRequired();

    builder.Property(p => p.Username)
        .HasMaxLength(DataSchemaConstants.UsernameLength)
        .IsRequired();

    builder.Property(p => p.HashedPassword)
        .HasMaxLength(DataSchemaConstants.HashedPasswordLength)
        .IsRequired();

    builder.Property(p => p.Bio)
        .HasMaxLength(DataSchemaConstants.BioLength)
        .IsRequired();

    builder.Property(p => p.Image)
        .HasMaxLength(DataSchemaConstants.ImageUrlLength);

    // Create unique indexes
    builder.HasIndex(p => p.Email)
        .IsUnique()
        .HasDatabaseName("IX_Users_Email");

    builder.HasIndex(p => p.Username)
        .IsUnique()
        .HasDatabaseName("IX_Users_Username");
  }
}
