using Server.Core.OrganizationAggregate;

namespace Server.Infrastructure.Data.Config;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
  public void Configure(EntityTypeBuilder<Organization> builder)
  {
    builder.Property(x => x.Name)
      .HasMaxLength(Organization.NameMaxLength)
      .IsRequired();

    builder.Property(x => x.Identifier)
      .HasMaxLength(Organization.IdentifierMaxLength)
      .IsRequired();

    builder.HasAlternateKey(x => x.Identifier);

    builder.HasIndex(x => x.Identifier)
      .IsUnique();
  }
}
