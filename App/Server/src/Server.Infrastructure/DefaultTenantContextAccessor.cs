using Finbuckle.MultiTenant.Abstractions;

namespace Server.Infrastructure;

// Phase 4: Provides a default TenantInfo for all operations.
// This allows ApplicationUser operations without full tenant resolution.
// Will be replaced with actual tenant resolution strategies in Phase 5+.
public class DefaultTenantContextAccessor : IMultiTenantContextAccessor
{
  private readonly IMultiTenantContext _context;

  public DefaultTenantContextAccessor()
  {
    // Create default TenantInfo
    // NOTE: Both Id and Identifier must be "" because MultiTenantIdentityDbContext uses Id
    // for the TenantId column
    var tenantInfo = new TenantInfo(string.Empty, string.Empty, "Default");

    _context = new MultiTenantContext<TenantInfo>(tenantInfo);
  }

  public IMultiTenantContext MultiTenantContext => _context;
}
