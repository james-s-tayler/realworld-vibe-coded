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

    // First, check if correlation ID is in the request header (set by client or Serilog enricher)
    if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("x-correlation-id", out var headerValue)
        && !string.IsNullOrWhiteSpace(headerValue))
    {
      var correlationId = headerValue.ToString();

      // Store it in Items for consistent access
      _httpContextAccessor.HttpContext.Items["CorrelationId"] = correlationId;
      return correlationId;
    }

    // Second, check if we've already generated/stored one in Items
    if (_httpContextAccessor.HttpContext.Items.TryGetValue("CorrelationId", out var itemValue)
        && itemValue is string storedId
        && !string.IsNullOrWhiteSpace(storedId))
    {
      return storedId;
    }

    // Finally, generate a new one and store it
    var newCorrelationId = Guid.NewGuid().ToString();
    _httpContextAccessor.HttpContext.Items["CorrelationId"] = newCorrelationId;
    return newCorrelationId;
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
