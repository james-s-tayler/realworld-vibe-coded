using Finbuckle.MultiTenant.Abstractions;

namespace Server.IntegrationTests.Data;

/// <summary>
/// Test helper for providing a fixed IMultiTenantContextAccessor implementation.
/// </summary>
internal class TestMultiTenantContextAccessor : IMultiTenantContextAccessor
{
  private readonly IMultiTenantContext _context;

  public TestMultiTenantContextAccessor(IMultiTenantContext context)
  {
    _context = context;
  }

  public IMultiTenantContext MultiTenantContext => _context;
}
