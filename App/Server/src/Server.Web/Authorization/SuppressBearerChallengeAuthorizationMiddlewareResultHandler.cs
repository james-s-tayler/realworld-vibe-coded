using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Server.Web.Authorization;

/// <summary>
/// Custom authorization result handler that suppresses Bearer token authentication challenges
/// to prevent "Headers are read-only" errors when response has already started.
/// </summary>
public class SuppressBearerChallengeAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
  private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

  public async Task HandleAsync(
    RequestDelegate next,
    HttpContext context,
    AuthorizationPolicy policy,
    PolicyAuthorizationResult authorizeResult)
  {
    // If authorization failed and we're using Bearer authentication, suppress the challenge
    if (!authorizeResult.Succeeded &&
        context.Request.Headers.Authorization.ToString().StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
    {
      // Skip the challenge to avoid "Headers are read-only" error
      // Just return 401 without setting WWW-Authenticate header
      context.Response.StatusCode = StatusCodes.Status401Unauthorized;
      return;
    }

    // Otherwise use default handling
    await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
  }
}
