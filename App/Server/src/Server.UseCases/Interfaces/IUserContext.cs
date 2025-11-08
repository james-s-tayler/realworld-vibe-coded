namespace Server.UseCases.Interfaces;

/// <summary>
/// Service for accessing the current authenticated user's context
/// </summary>
public interface IUserContext
{
  string GetCorrelationId();

  /// <summary>
  /// Gets the current authenticated user's ID
  /// </summary>
  /// <returns>The user ID if authenticated and valid, otherwise null</returns>
  Guid? GetCurrentUserId();

  /// <summary>
  /// Gets the current authenticated user's ID, throwing an exception if not authenticated
  /// </summary>
  /// <returns>The authenticated user's ID</returns>
  /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated</exception>
  Guid GetRequiredCurrentUserId();

  /// <summary>
  /// Determines if the current user is authenticated
  /// </summary>
  /// <returns>True if authenticated, false otherwise</returns>
  bool IsAuthenticated();

  /// <summary>
  /// Gets the current JWT token from the Authorization header
  /// </summary>
  /// <returns>The JWT token if present, otherwise null</returns>
  string? GetCurrentToken();

  /// <summary>
  /// Gets the current authenticated user's username
  /// </summary>
  /// <returns>The username if authenticated, otherwise null</returns>
  string? GetCurrentUsername();
}
