namespace Server.Core.Interfaces;

/// <summary>
/// Service for accessing the current authenticated user's context
/// </summary>
public interface ICurrentUserService
{
  /// <summary>
  /// Gets the current authenticated user's ID
  /// </summary>
  /// <returns>The user ID if authenticated and valid, otherwise null</returns>
  int? GetCurrentUserId();

  /// <summary>
  /// Gets the current authenticated user's ID, throwing an exception if not authenticated
  /// </summary>
  /// <returns>The authenticated user's ID</returns>
  /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated</exception>
  int GetRequiredCurrentUserId();

  /// <summary>
  /// Determines if the current user is authenticated
  /// </summary>
  /// <returns>True if authenticated, false otherwise</returns>
  bool IsAuthenticated();
}
