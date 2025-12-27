using Ardalis.ListStartupServices;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;
using Server.Web.Infrastructure;

namespace Server.Web.Configurations;

public static class MiddlewareConfig
{
  public static async Task<IApplicationBuilder> UseAppMiddlewareAndSeedDatabase(this WebApplication app)
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

    app.UseExceptionHandler();
    app.UseFastEndpoints(config =>
    {
      config.Errors.UseProblemDetails();
      config.Endpoints.Configurator = ep =>
      {
        ep.PostProcessor<GlobalExceptionHandler>(Order.After);
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
    app.UseSwaggerGen(); // Includes AddFileServer and static files middleware
    app.UseHttpsRedirection(); // Note this will drop Authorization headers
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

    await SeedDatabase(app);

    return app;
  }

  private static async Task SeedDatabase(WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
      var context = services.GetRequiredService<AppDbContext>();

      await context.Database.MigrateAsync();
      await SeedData.InitializeAsync(context);
    }
    catch (Exception ex)
    {
      var logger = services.GetRequiredService<ILogger<Program>>();
      logger.LogError(ex, "An error occurred seeding the DB. {exceptionMessage}", ex.Message);
    }
  }
}
