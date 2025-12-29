using Finbuckle.MultiTenant.Abstractions;
using Server.Infrastructure.Data;
using Server.UseCases.Identity;

namespace Server.Infrastructure.Identity;

public class TenantStore : ITenantStore
{
  private readonly AppDbContext _dbContext;

  public TenantStore(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task CreateTenantAsync(string tenantId, string tenantName, CancellationToken cancellationToken = default)
  {
    var tenantInfo = new TenantInfo(tenantId, tenantId, tenantName);
    _dbContext.TenantInfo.Add(tenantInfo);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}
