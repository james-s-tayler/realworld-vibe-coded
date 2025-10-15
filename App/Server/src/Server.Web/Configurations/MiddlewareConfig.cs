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
          c.Errors.StatusCode = 400;
          c.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
          {
            var errors = failures.Select(failure =>
            {
              var propertyName = failure.PropertyName;
              // Handle nested properties like User.Username -> username
              if (propertyName.Contains('.'))
              {
                var parts = propertyName.Split('.');
                propertyName = parts[parts.Length - 1];
              }
              return new
              {
                name = propertyName.ToLower(),
                reason = failure.ErrorMessage
              };
            }).ToList();

            return new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
              Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
              Title = "One or more validation errors occurred.",
              Status = statusCode,
              Instance = ctx.Request.Path,
              Extensions =
              {
                ["errors"] = errors,
                ["traceId"] = ctx.TraceIdentifier
              }
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
