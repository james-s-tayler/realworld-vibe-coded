using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Server.Web.Authorization;

/// <summary>
/// Custom authorization result handler that suppresses Bearer token authentication challenges
/// to prevent "Headers are read-only" errors when response has already started.
/// </summary>
public sealed class SuppressBearerChallengeAuthorizationMiddlewareResultHandler
    : IAuthorizationMiddlewareResultHandler
{
  private readonly Microsoft.AspNetCore.Authorization.Policy.AuthorizationMiddlewareResultHandler _defaultHandler = new();
  private readonly ILogger<SuppressBearerChallengeAuthorizationMiddlewareResultHandler> _logger;

  public SuppressBearerChallengeAuthorizationMiddlewareResultHandler(
      ILogger<SuppressBearerChallengeAuthorizationMiddlewareResultHandler> logger)
  {
    _logger = logger;
  }

  public async Task HandleAsync(
      RequestDelegate next,
      HttpContext context,
      AuthorizationPolicy policy,
      PolicyAuthorizationResult authorizeResult)
  {
    // 1. If authorized, just continue
    if (authorizeResult.Succeeded)
    {
      await next(context);
      return;
    }

    // 2. If response already started, we can't safely change it
    if (context.Response.HasStarted)
    {
      _logger.LogWarning(
          "Authorization failed but the response has already started. " +
          "Skipping challenge/forbid. Path: {Path}",
          context.Request.Path);

      // Nothing more we can safely do
      return;
    }

    // 3. Weird edge-case: failed without Challenged/Forbidden flags? Let default handle it.
    if (!authorizeResult.Challenged && !authorizeResult.Forbidden)
    {
      await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
      return;
    }

    // 4. From here on we *own* the response: NEVER call ChallengeAsync/ForbidAsync.
    //    That would invoke JwtBearer/BearerToken handlers and reintroduce the bug.

    if (authorizeResult.Forbidden)
    {
      var failures = new List<ValidationFailure>
            {
                new("authorization", "Forbidden"),
            };

      await context.Response.SendErrorsAsync(
          failures,
          StatusCodes.Status403Forbidden);

      return;
    }

    // authorizeResult.Challenged
    {
      var failures = new List<ValidationFailure>
            {
                new("authorization", "Unauthorized"),
            };

      await context.Response.SendErrorsAsync(
          failures,
          StatusCodes.Status401Unauthorized);

      return;
    }
  }
}
