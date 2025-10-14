using Ardalis.ListStartupServices;
using Server.Infrastructure.Data;

namespace Server.Web.Configurations;

public static class MiddlewareConfig
{
  public static async Task<IApplicationBuilder> UseAppMiddlewareAndSeedDatabase(this WebApplication app)
  {
    if (app.Environment.IsDevelopment())
    {
      app.UseShowAllServicesMiddleware(); // see https://github.com/ardalis/AspNetCoreStartupServices
      app.UseCors("AllowLocalhost");
    }
    else
    {
      app.UseHsts();
    }

    app.UseFastEndpoints(c =>
        {
          c.Errors.StatusCode = 422;
          c.Binding.ValueParserFor<string>(rawValue => rawValue); // Bind all query params as raw strings
          c.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
          {
            var errorBody = new List<string>();
            foreach (var failure in failures)
            {
              // Handle nested properties like Article.Title -> title
              var propertyName = failure.PropertyName.ToLower();
              if (propertyName.Contains('.'))
              {
                // Split on dot and take the last part
                var parts = propertyName.Split('.');
                propertyName = parts[parts.Length - 1];
              }

              errorBody.Add($"{propertyName} {failure.ErrorMessage}");
            }
            return new
            {
              errors = new { body = errorBody }
            };
          };
        })
        .UseSwaggerGen(); // Includes AddFileServer and static files middleware

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
}
