namespace Server.SharedKernel.Identity;

/// <summary>
/// Interface for checking user email existence across all tenants.
/// This bypasses multi-tenant query filters to enable cross-tenant validation.
/// </summary>
public interface IUserEmailChecker
{
  /// <summary>
  /// Checks if a user with the specified email exists in any tenant.
  /// </summary>
  /// <param name="email">The email address to check (will be normalized internally)</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if a user with this email exists in any tenant, false otherwise</returns>
  Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

  /// <summary>
  /// Finds a user by email across all tenants.
  /// </summary>
  /// <typeparam name="TUser">The user type</typeparam>
  /// <param name="email">The email address to search for (will be normalized internally)</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The user if found, null otherwise</returns>
  Task<TUser?> FindByEmailAsync<TUser>(string email, CancellationToken cancellationToken = default) where TUser : class;

  /// <summary>
  /// Gets the TenantId shadow property value for a user.
  /// </summary>
  /// <typeparam name="TUser">The user type</typeparam>
  /// <param name="user">The user entity</param>
  /// <returns>The TenantId value from the shadow property</returns>
  string GetTenantId<TUser>(TUser user) where TUser : class;
}
