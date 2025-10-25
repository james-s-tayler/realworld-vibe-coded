using Audit.Core;
using Microsoft.AspNetCore.Http;
using Server.Core.Interfaces;

namespace Server.Infrastructure.Data;

public static class AuditConfiguration
{
  public static void ConfigureAudit(IServiceProvider serviceProvider, string auditLogsPath)
  {
    // Ensure the audit logs directory exists
    if (!Directory.Exists(auditLogsPath))
    {
      Directory.CreateDirectory(auditLogsPath);
    }

    Audit.Core.Configuration.Setup()
      .UseFileLogProvider(config => config
        .Directory(auditLogsPath)
        .FilenameBuilder(auditEvent =>
          $"audit_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.json"))
      .WithCreationPolicy(EventCreationPolicy.InsertOnEnd)
      .WithAction(action => action
        .OnScopeCreated(scope =>
        {
          // Add current user information to the audit event
          var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
          if (httpContextAccessor?.HttpContext != null)
          {
            // Get ICurrentUserService from the scoped service provider (RequestServices) instead of root provider
            var currentUserService = httpContextAccessor.HttpContext.RequestServices.GetService<ICurrentUserService>();
            var userId = currentUserService?.GetCurrentUserId();
            var username = httpContextAccessor.HttpContext.User?.Identity?.Name;

            if (userId.HasValue)
            {
              scope.SetCustomField("UserId", userId.Value);
            }

            if (!string.IsNullOrEmpty(username))
            {
              scope.SetCustomField("Username", username);
            }
            else if (userId.HasValue)
            {
              // If username is not in claims, try to get it from ClaimTypes.NameIdentifier
              var userIdClaim = httpContextAccessor.HttpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
              if (userIdClaim != null)
              {
                scope.SetCustomField("Username", $"UserId_{userIdClaim.Value}");
              }
            }
          }
        }));
  }
}
