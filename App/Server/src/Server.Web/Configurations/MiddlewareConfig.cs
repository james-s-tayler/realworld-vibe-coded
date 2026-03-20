using Ardalis.ListStartupServices;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;
using Server.Web.Infrastructure;

namespace Server.Web.Configurations;

public static class MiddlewareConfig
{
  public static IApplicationBuilder UseFastEndpointsMiddleware(this WebApplication app)
  {
    app.UseExceptionHandler();
    app.UseFastEndpoints(config =>
    {
      config.Errors.UseProblemDetails();
      config.Endpoints.Configurator = ep =>
      {
        ep.PostProcessor<GlobalExceptionHandler>(Order.After);
        ep.Description(d => d
          .ProducesProblemDetails(StatusCodes.Status401Unauthorized)
          .ProducesProblemDetails(StatusCodes.Status404NotFound)
          .ProducesProblemDetails(StatusCodes.Status403Forbidden)
          .ProducesProblemDetails(StatusCodes.Status409Conflict)
          .ProducesProblemDetails(StatusCodes.Status500InternalServerError));
      };
      if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
      {
        config.Endpoints.Filter = ep =>
        {
          foreach (var route in ep.Routes)
          {
            if (route.StartsWith(DevOnly.Configuration.DevOnly.ROUTE))
            {
              return false; // don't register these endpoints in non-development environments
            }
          }

          return true;
        };
      }
    });

    return app;
  }

  public static async Task<IApplicationBuilder> UseAppMiddlewareAndSeedDatabaseAsync(this WebApplication app)
  {
    // Configure Audit.NET - use path from configuration
    var auditLogsPath = app.Configuration["Audit:LogsPath"] ?? "Logs/Audit";

    // Make path absolute if it's relative
    if (!Path.IsPathRooted(auditLogsPath))
    {
      auditLogsPath = Path.Combine(Directory.GetCurrentDirectory(), auditLogsPath);
    }

    AuditConfiguration.ConfigureAudit(app.Services, auditLogsPath);

    if (app.Environment.IsDevelopment())
    {
      app.UseShowAllServicesMiddleware(); // see https://github.com/ardalis/AspNetCoreStartupServices
      app.UseCors("AllowLocalhost");
    }
    else
    {
      app.UseHsts();
    }

    app.UseSwaggerGen(); // Includes AddFileServer and static files middleware
    app.UseHttpsRedirection(); // Note this will drop Authorization headers
    app.UseMultiTenant();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery(); // Enable CSRF protection for cookie-based authentication

    // Map health check endpoints
    // /health/live - Liveness probe (always returns healthy if app is running)
    // /health/ready - Readiness probe (checks database connection and schema)
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
      Predicate = _ => false, // No checks, just confirm app is alive
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
      Predicate = check => check.Tags.Contains("ready"), // Only run readiness checks (database)
    });

    await RunMigrationsAsync(app);

    return app;
  }

  private static async Task RunMigrationsAsync(WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
      var tenantStoreContext = services.GetRequiredService<TenantStoreDbContext>();
      await tenantStoreContext.Database.MigrateAsync();

      var context = services.GetRequiredService<AppDbContext>();
      await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
      var logger = services.GetRequiredService<ILogger<Program>>();
      logger.LogError(ex, "An error occurred migrating the DB. {exceptionMessage}", ex.Message);
      throw;
    }
  }
}
