using Server.Core.UserAggregate;

namespace Server.Infrastructure.Data.Config;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.Property(p => p.Email)
        .HasMaxLength(User.EmailMaxLength)
        .IsRequired();

    builder.Property(p => p.Username)
        .HasMaxLength(User.UsernameMaxLength)
        .IsRequired();

    builder.Property(p => p.HashedPassword)
        .HasMaxLength(User.HashedPasswordMaxLength)
        .IsRequired();

    builder.Property(p => p.Bio)
        .HasMaxLength(User.BioMaxLength)
        .IsRequired();

    builder.Property(p => p.Image)
        .HasMaxLength(User.ImageUrlMaxLength);

    // Create unique indexes
    builder.HasIndex(p => p.Email)
        .IsUnique()
        .HasDatabaseName("IX_Users_Email");

    builder.HasIndex(p => p.Username)
        .IsUnique()
        .HasDatabaseName("IX_Users_Username");
  }
}
