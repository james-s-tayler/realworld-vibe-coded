using Server.Core.IdentityAggregate;

namespace Server.Infrastructure.Data.Config;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
  public void Configure(EntityTypeBuilder<ApplicationUser> builder)
  {
    builder.Property(p => p.Bio)
        .HasMaxLength(ApplicationUser.BioMaxLength);

    builder.Property(p => p.Image)
        .HasMaxLength(ApplicationUser.ImageUrlMaxLength);

    // TenantId is added automatically by MultiTenantIdentityDbContext as a shadow property (string type)
    // Configure relationship to Organization using TenantId -> Organization.Identifier
    builder.HasOne(u => u.Organization)
        .WithMany()
        .HasForeignKey("TenantId")
        .HasPrincipalKey(o => o.Identifier)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex("TenantId");

    // Note: Following/Followers relationships will use the existing UserFollowing table
    // These will be configured once we integrate ApplicationUser with the existing User relationships
  }
}
