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
  private readonly IMultiTenantContextAccessor _multiTenantContextAccessor;

  public TenantAssigner(
    UserManager<ApplicationUser> userManager,
    AppDbContext dbContext,
    IMultiTenantContextAccessor multiTenantContextAccessor)
  {
    _userManager = userManager;
    _dbContext = dbContext;
    _multiTenantContextAccessor = multiTenantContextAccessor;
  }

  public async Task SetTenantIdAsync(Guid userId, string tenantIdentifier, CancellationToken cancellationToken = default)
  {
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null)
    {
      throw new InvalidOperationException($"User with ID {userId} not found");
    }

    // Temporarily set the tenant context to the organization's identifier
    // This allows Finbuckle to accept the TenantId assignment
    var originalContext = _multiTenantContextAccessor.MultiTenantContext;
    var newTenantInfo = new TenantInfo(tenantIdentifier, tenantIdentifier, "Organization");
    var newContext = new MultiTenantContext<TenantInfo>(newTenantInfo);

    // Use reflection to set the context since AsyncLocalMultiTenantContextAccessor uses AsyncLocal
    var contextAccessorType = _multiTenantContextAccessor.GetType();
    var asyncLocalField = contextAccessorType.GetField("_asyncLocalContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    if (asyncLocalField != null)
    {
      var asyncLocal = asyncLocalField.GetValue(_multiTenantContextAccessor);
      if (asyncLocal != null)
      {
        var asyncLocalType = asyncLocal.GetType();
        var valueProperty = asyncLocalType.GetProperty("Value");
        valueProperty?.SetValue(asyncLocal, newContext);
      }
    }

    try
    {
      // Set TenantId shadow property
      var entry = _dbContext.Entry(user);
      entry.Property("TenantId").CurrentValue = tenantIdentifier;
      await _dbContext.SaveChangesAsync(cancellationToken);
    }
    finally
    {
      // Restore original context
      if (asyncLocalField != null)
      {
        var asyncLocal = asyncLocalField.GetValue(_multiTenantContextAccessor);
        if (asyncLocal != null)
        {
          var asyncLocalType = asyncLocal.GetType();
          var valueProperty = asyncLocalType.GetProperty("Value");
          valueProperty?.SetValue(asyncLocal, originalContext);
        }
      }
    }
  }

  public Task SetTenantContextAsync(string tenantIdentifier, CancellationToken cancellationToken = default)
  {
    // Set the tenant context to the organization's identifier
    // This allows role operations to work within the tenant scope
    var newTenantInfo = new TenantInfo(tenantIdentifier, tenantIdentifier, "Organization");
    var newContext = new MultiTenantContext<TenantInfo>(newTenantInfo);

    // Use reflection to set the context since AsyncLocalMultiTenantContextAccessor uses AsyncLocal
    var contextAccessorType = _multiTenantContextAccessor.GetType();
    var asyncLocalField = contextAccessorType.GetField("_asyncLocalContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    if (asyncLocalField != null)
    {
      var asyncLocal = asyncLocalField.GetValue(_multiTenantContextAccessor);
      if (asyncLocal != null)
      {
        var asyncLocalType = asyncLocal.GetType();
        var valueProperty = asyncLocalType.GetProperty("Value");
        valueProperty?.SetValue(asyncLocal, newContext);
      }
    }

    return Task.CompletedTask;
  }
}
