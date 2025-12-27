using Finbuckle.MultiTenant.Abstractions;

namespace Server.IntegrationTests.Data;

/// <summary>
/// Test implementation of IMultiTenantContextAccessor that provides a default tenant context
/// </summary>
internal class TestMultiTenantContextAccessor : IMultiTenantContextAccessor
{
  private readonly IMultiTenantContext _context;

  public TestMultiTenantContextAccessor()
  {
    var tenantInfo = new TenantInfo(string.Empty, string.Empty, "Test");
    _context = new MultiTenantContext<TenantInfo>(tenantInfo);
  }

  public IMultiTenantContext MultiTenantContext => _context;
}
