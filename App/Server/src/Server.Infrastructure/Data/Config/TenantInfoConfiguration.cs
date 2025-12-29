using Finbuckle.MultiTenant.Abstractions;

namespace Server.Infrastructure.Data.Config;

/// <summary>
/// Configuration for Finbuckle's TenantInfo.
/// </summary>
public class TenantInfoConfiguration : IEntityTypeConfiguration<TenantInfo>
{
  public void Configure(EntityTypeBuilder<TenantInfo> builder)
  {
    builder.HasKey(t => t.Id);

    builder.Property(t => t.Id)
        .IsRequired();

    builder.Property(t => t.Identifier)
        .IsRequired();

    builder.Property(t => t.Name);
  }
}
