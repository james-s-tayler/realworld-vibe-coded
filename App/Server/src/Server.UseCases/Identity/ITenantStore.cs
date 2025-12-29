namespace Server.UseCases.Identity;

/// <summary>
/// Interface for managing tenant information.
/// Abstracts tenant storage operations from the domain layer.
/// </summary>
public interface ITenantStore
{
  /// <summary>
  /// Creates a new tenant with the specified ID and name.
  /// </summary>
  /// <param name="tenantId">Unique identifier for the tenant</param>
  /// <param name="tenantName">Display name for the tenant</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Task representing the asynchronous operation</returns>
  Task CreateTenantAsync(string tenantId, string tenantName, CancellationToken cancellationToken = default);
}
