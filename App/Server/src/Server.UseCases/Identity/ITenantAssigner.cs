namespace Server.UseCases.Identity;

public interface ITenantAssigner
{
  Task SetTenantIdAsync(Guid userId, string tenantIdentifier, CancellationToken cancellationToken = default);
}
