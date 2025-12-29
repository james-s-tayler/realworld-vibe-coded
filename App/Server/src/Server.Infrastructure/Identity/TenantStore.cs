using Finbuckle.MultiTenant.Abstractions;
using Server.Infrastructure.Data;
using Server.UseCases.Identity;

namespace Server.Infrastructure.Identity;

public class TenantStore : ITenantStore
{
  private readonly AppDbContext _dbContext;
  private readonly ILogger<TenantStore> _logger;

  public TenantStore(AppDbContext dbContext, ILogger<TenantStore> logger)
  {
    _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public async Task CreateTenantAsync(string tenantId, string tenantName, CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("TenantStore.CreateTenantAsync called with tenantId={TenantId}, tenantName={TenantName}", tenantId, tenantName);

    var tenantInfo = new TenantInfo(tenantId, tenantId, tenantName);
    _logger.LogInformation("Created TenantInfo instance");

    var tenantInfoSet = _dbContext.Set<TenantInfo>();
    _logger.LogInformation("Got DbSet<TenantInfo>, adding tenant");

    tenantInfoSet.Add(tenantInfo);
    _logger.LogInformation("Added TenantInfo to DbSet, saving changes");

    await _dbContext.SaveChangesAsync(cancellationToken);
    _logger.LogInformation("Successfully saved TenantInfo");
  }
}
