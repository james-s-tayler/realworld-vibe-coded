using Ardalis.ListStartupServices;
using Server.Infrastructure.Data;
using Server.Web.Infrastructure;

namespace Server.Web.Configurations;

public static class MiddlewareConfig
{
  public static async Task<IApplicationBuilder> UseAppMiddlewareAndSeedDatabase(this WebApplication app)
  {
    // Configure Audit.NET - use absolute path from repository root
    var auditLogsPath = GetRepositoryPath(app.Environment.ContentRootPath, "Logs", "Audit");
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
      if (!app.Environment.IsDevelopment())
      {
        config.Endpoints.Filter = ep =>
        {
          if (ep.Routes.Contains("/api/error-test"))
          {
            return false; // don't register these endpoints in non-development environments
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
      await context.Database.EnsureCreatedAsync();
      await SeedData.InitializeAsync(context);
    }
    catch (Exception ex)
    {
      var logger = services.GetRequiredService<ILogger<Program>>();
      logger.LogError(ex, "An error occurred seeding the DB. {exceptionMessage}", ex.Message);
    }
  }

  private static string GetRepositoryPath(string startPath, params string[] pathSegments)
  {
    // Find the repository root by looking for Directory.Build.props (marker file in App/Server)
    var currentDir = new DirectoryInfo(startPath);

    while (currentDir != null)
    {
      // Check if Directory.Build.props exists (this is in App/Server)
      var markerFile = Path.Combine(currentDir.FullName, "Directory.Build.props");
      if (File.Exists(markerFile))
      {
        // Go up two levels to get to repository root (from App/Server to root)
        var repoRoot = currentDir.Parent?.Parent?.FullName ?? currentDir.FullName;
        return Path.Combine(new[] { repoRoot }.Concat(pathSegments).ToArray());
      }

      currentDir = currentDir.Parent;
    }

    // Fallback to relative path if marker not found
    return Path.Combine(new[] { startPath }.Concat(pathSegments).ToArray());
  }
}
