using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Server.UseCases.Interfaces;

namespace Server.Infrastructure.Services;

/// <summary>
/// Service for accessing the current authenticated user's context
/// </summary>
public class UserContext : IUserContext
{
  private readonly IHttpContextAccessor _httpContextAccessor;

  public UserContext(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public string GetCorrelationId()
  {
    if (_httpContextAccessor.HttpContext == null)
    {
      return Guid.Empty.ToString();
    }

    if (!_httpContextAccessor.HttpContext.Items.ContainsKey("CorrelationId"))
    {
      _httpContextAccessor.HttpContext.Items["CorrelationId"] = Guid.NewGuid().ToString();
    }

    return _httpContextAccessor.HttpContext.Items["CorrelationId"] as string ?? Guid.Empty.ToString();
  }

  /// <summary>
  /// Gets the current authenticated user's ID
  /// </summary>
  /// <returns>The user ID if authenticated and valid, otherwise null</returns>
  public Guid? GetCurrentUserId()
  {
    var httpContext = _httpContextAccessor.HttpContext;
    if (httpContext?.User?.Identity?.IsAuthenticated != true)
    {
      return null;
    }

    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
    {
      return userId;
    }

    return null;
  }

  /// <summary>
  /// Gets the current authenticated user's ID, throwing an exception if not authenticated
  /// </summary>
  /// <returns>The authenticated user's ID</returns>
  /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated</exception>
  public Guid GetRequiredCurrentUserId()
  {
    var userId = GetCurrentUserId();
    if (!userId.HasValue)
    {
      throw new UnauthorizedAccessException("User is not authenticated or user ID is invalid");
    }

    return userId.Value;
  }

  /// <summary>
  /// Determines if the current user is authenticated
  /// </summary>
  /// <returns>True if authenticated, false otherwise</returns>
  public bool IsAuthenticated()
  {
    return GetCurrentUserId().HasValue;
  }

  /// <summary>
  /// Gets the current JWT token from the Authorization header
  /// </summary>
  /// <returns>The JWT token if present, otherwise null</returns>
  public string? GetCurrentToken()
  {
    var httpContext = _httpContextAccessor.HttpContext;
    if (httpContext == null)
    {
      return null;
    }

    var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();
    if (string.IsNullOrEmpty(authorizationHeader))
    {
      return null;
    }

    // The token format is "Token <jwt-token>"
    if (authorizationHeader.StartsWith("Token ", StringComparison.OrdinalIgnoreCase))
    {
      return authorizationHeader.Substring("Token ".Length).Trim();
    }

    return null;
  }

  /// <summary>
  /// Gets the current authenticated user's username
  /// </summary>
  /// <returns>The username if authenticated, otherwise null</returns>
  public string? GetCurrentUsername()
  {
    var httpContext = _httpContextAccessor.HttpContext;
    if (!IsAuthenticated())
    {
      return null;
    }

    var usernameClaim = httpContext!.User.FindFirst(ClaimTypes.Name);
    return usernameClaim?.Value;
  }
}
