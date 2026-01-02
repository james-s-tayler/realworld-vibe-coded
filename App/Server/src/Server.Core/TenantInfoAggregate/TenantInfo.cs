using Server.SharedKernel.Persistence;

namespace Server.Core.TenantInfoAggregate;

/// <summary>
/// Represents information about a tenant in the multi-tenant system.
/// Derives from Finbuckle.MultiTenant.Abstractions.TenantInfo which provides Id, Identifier, and Name properties.
/// </summary>
public record TenantInfo : Finbuckle.MultiTenant.Abstractions.TenantInfo, IAggregateRoot
{
  /// <summary>
  /// Initializes a new instance of the TenantInfo record.
  /// </summary>
  /// <param name="id">Unique identifier for the tenant (never changes)</param>
  /// <param name="identifier">Tenant identifier used for resolution (can change if needed)</param>
  /// <param name="name">Display name for the tenant</param>
  public TenantInfo(string id, string identifier, string name)
    : base(id, identifier, name)
  {
  }

  /// <summary>
  /// Parameterless constructor required by EF Core.
  /// </summary>
  public TenantInfo() : base(string.Empty, string.Empty, string.Empty)
  {
  }
}
