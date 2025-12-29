using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.Infrastructure.Data;
using Server.UseCases.Identity;

namespace Server.Infrastructure.Identity;

public class TenantAssigner : ITenantAssigner
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly AppDbContext _dbContext;
  private readonly IMultiTenantContextSetter _multiTenantContextSetter;

  public TenantAssigner(
    UserManager<ApplicationUser> userManager,
    AppDbContext dbContext,
    IMultiTenantContextSetter multiTenantContextSetter)
  {
    _userManager = userManager;
    _dbContext = dbContext;
    _multiTenantContextSetter = multiTenantContextSetter;
  }

  public async Task SetTenantIdAsync(Guid userId, string tenantIdentifier, CancellationToken cancellationToken = default)
  {
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null)
    {
      throw new InvalidOperationException($"User with ID {userId} not found");
    }

    // Set TenantId shadow property
    var entry = _dbContext.Entry(user);
    entry.Property("TenantId").CurrentValue = tenantIdentifier;
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public Task SetTenantContextAsync(string tenantIdentifier, CancellationToken cancellationToken = default)
  {
    // Set the tenant context using IMultiTenantContextSetter
    // This allows role operations to work within the tenant scope
    var newTenantInfo = new TenantInfo(tenantIdentifier, tenantIdentifier, "Organization");
    var newContext = new MultiTenantContext<TenantInfo>(newTenantInfo);

    _multiTenantContextSetter.MultiTenantContext = newContext;

    return Task.CompletedTask;
  }
}
