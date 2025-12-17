using Server.Core.IdentityAggregate;

namespace Server.Infrastructure.Data.Config;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
  public void Configure(EntityTypeBuilder<ApplicationUser> builder)
  {
    // Override default ASP.NET Identity UserName max length
    builder.Property(p => p.UserName)
        .HasMaxLength(ApplicationUser.UsernameMaxLength);

    builder.Property(p => p.Bio)
        .HasMaxLength(ApplicationUser.BioMaxLength);

    builder.Property(p => p.Image)
        .HasMaxLength(ApplicationUser.ImageUrlMaxLength);

    // Note: Following/Followers relationships will use the existing UserFollowing table
    // These will be configured once we integrate ApplicationUser with the existing User relationships
  }
}
