﻿using Ardalis.ListStartupServices;
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

    await SeedDatabase(app);

    return app;
  }

  static async Task SeedDatabase(WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
      var context = services.GetRequiredService<AppDbContext>();
      //          await context.Database.MigrateAsync();
      // PV042: EnsureCreatedAsync is acceptable here as this is development-time database seeding.
      // In production, migrations should be used instead. This code should be guarded by environment checks.
#pragma warning disable PV042
      await context.Database.EnsureCreatedAsync();
#pragma warning restore PV042
      await SeedData.InitializeAsync(context);
    }
    catch (Exception ex)
    {
      var logger = services.GetRequiredService<ILogger<Program>>();
      logger.LogError(ex, "An error occurred seeding the DB. {exceptionMessage}", ex.Message);
    }
  }
}
