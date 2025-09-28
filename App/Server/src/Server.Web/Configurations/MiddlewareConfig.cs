using Ardalis.ListStartupServices;
using Server.Infrastructure.Data;
using Server.Web.Infrastructure;

namespace Server.Web.Configurations;

public static class MiddlewareConfig
{
  public static async Task<IApplicationBuilder> UseAppMiddlewareAndSeedDatabase(this WebApplication app)
  {
    if (app.Environment.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
      app.UseShowAllServicesMiddleware(); // see https://github.com/ardalis/AspNetCoreStartupServices
    }
    else
    {
      app.UseDefaultExceptionHandler(); // from FastEndpoints
      app.UseHsts();
    }

    app.UseFastEndpoints()
        .UseSwaggerGen(); // Includes AddFileServer and static files middleware

    app.UseHttpsRedirection(); // Note this will drop Authorization headers
    app.UseAuthentication();
    app.UseAuthorization();

    await SeedDatabase(app);

    // Configure SPA serving
    if (app.Environment.IsDevelopment())
    {
      // In development, proxy to Vite dev server
      app.UseSpaDevServer("http://localhost:5173");
    }
    else
    {
      // In production, serve static files and fallback to SPA
      app.UseDefaultFiles(); // Serve index.html by default
      app.UseStaticFiles(new StaticFileOptions
      {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
          Path.Combine(Directory.GetCurrentDirectory(), "../Client/dist")),
        RequestPath = ""
      });
      app.MapFallbackToFile("index.html"); // SPA fallback routing
    }

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
}
