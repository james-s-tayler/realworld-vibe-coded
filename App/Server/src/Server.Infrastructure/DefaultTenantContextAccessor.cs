using Finbuckle.MultiTenant.Abstractions;

namespace Server.Infrastructure;

/// <summary>
/// Phase 4: Provides a default TenantInfo for all operations.
/// This allows ApplicationUser operations without full tenant resolution.
/// Will be replaced with actual tenant resolution strategies in Phase 5+.
/// </summary>
public class DefaultTenantContextAccessor : IMultiTenantContextAccessor
{
  private readonly IMultiTenantContext _context;

  public DefaultTenantContextAccessor()
  {
    // Create default TenantInfo matching the default Organization from migration
    var tenantInfo = new TenantInfo("00000000-0000-0000-0000-000000000001", string.Empty, "Default");

    _context = new MultiTenantContext<TenantInfo>(tenantInfo);
  }

  public IMultiTenantContext MultiTenantContext => _context;
}
