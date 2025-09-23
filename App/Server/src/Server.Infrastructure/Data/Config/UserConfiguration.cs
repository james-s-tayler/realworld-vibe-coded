using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Core.UserAggregate;

namespace Server.Infrastructure.Data.Config;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.Property(p => p.Email)
        .HasMaxLength(DataSchemaConstants.EMAIL_LENGTH)
        .IsRequired();

    builder.Property(p => p.Username)
        .HasMaxLength(DataSchemaConstants.USERNAME_LENGTH)
        .IsRequired();

    builder.Property(p => p.HashedPassword)
        .HasMaxLength(DataSchemaConstants.HASHED_PASSWORD_LENGTH)
        .IsRequired();

    builder.Property(p => p.Bio)
        .HasMaxLength(DataSchemaConstants.BIO_LENGTH)
        .IsRequired();

    builder.Property(p => p.Image)
        .HasMaxLength(DataSchemaConstants.IMAGE_URL_LENGTH);

    // Create unique indexes
    builder.HasIndex(p => p.Email)
        .IsUnique()
        .HasDatabaseName("IX_Users_Email");

    builder.HasIndex(p => p.Username)
        .IsUnique()
        .HasDatabaseName("IX_Users_Username");
  }
}
