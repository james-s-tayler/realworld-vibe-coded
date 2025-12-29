using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores;

namespace Server.Infrastructure.Data;

/// <summary>
/// Database context for storing tenant information.
/// This is separate from the main AppDbContext and stores tenant metadata.
/// </summary>
public class TenantStoreDbContext : EFCoreStoreDbContext<TenantInfo>
{
  public TenantStoreDbContext(DbContextOptions<TenantStoreDbContext> options)
    : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Finbuckle's base class configures the TenantInfo entity with proper constraints
    // We just need to ensure the migrations assembly is set correctly
  }
}
