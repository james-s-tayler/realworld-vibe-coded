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

    builder.Property<string>("TenantId")
        .IsRequired();

    builder.HasIndex("TenantId");
  }
}
