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
}
