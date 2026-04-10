using Finbuckle.MultiTenant.Abstractions;
using Microsoft.FeatureManagement.FeatureFilters;
using TenantInfo = Server.Core.TenantInfoAggregate.TenantInfo;

namespace Server.Web.Services;

public class TenantTargetingContextAccessor(
  IMultiTenantContextAccessor<TenantInfo> multiTenantContextAccessor) : ITargetingContextAccessor
{
  public ValueTask<TargetingContext> GetContextAsync()
  {
    var tenantId = multiTenantContextAccessor.MultiTenantContext?.TenantInfo?.Id ?? string.Empty;

    return ValueTask.FromResult(new TargetingContext
    {
      UserId = tenantId,
      Groups = [],
    });
  }
}
