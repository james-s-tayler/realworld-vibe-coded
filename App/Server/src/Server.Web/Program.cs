using Server.Web.Configurations;
using Server.Web.DevOnly.Configuration;
using Server.Web.Infrastructure;

// setup the app
var builder = WebApplication.CreateBuilder(args);

var logger = Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateLogger();

logger.Information("Starting web host");

builder.AddLoggerConfigs();

var appLogger = new SerilogLoggerFactory(logger)
    .CreateLogger<Program>();

builder.Services.AddOptionConfigs(builder.Configuration, appLogger, builder);
builder.Services.AddServiceConfigs(appLogger, builder);


builder.Services.AddFastEndpoints(o =>
                {
                  o.Assemblies = [typeof(Program).Assembly, typeof(Server.Web.DevOnly.Endpoints.ThrowInEndpoint).Assembly];
                })
                .SwaggerDocument(o =>
                {
                  o.ShortSchemaNames = true;
                  o.DocumentSettings = s =>
                  {
                    // Add Bearer token auth (matches IdentityConstants.BearerScheme)
                    s.AddAuth(Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme, new()
                    {
                      Type = NSwag.OpenApiSecuritySchemeType.Http,
                      Scheme = "Bearer",
                      Description = "ASP.NET Identity bearer token authentication. Enter your token in the text input below.",
                    });
                  };
                });

// Configure JSON serialization options
builder.Services.ConfigureHttpJsonOptions(options =>
{
  options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
  options.SerializerOptions.Converters.Add(new UtcDateTimeConverter());
  options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;

  // Don't ignore null values - we need them in the API response
  // options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

app.UseStaticFiles();
await app.UseAppMiddlewareAndSeedDatabase();

// Only serve SPA fallback for non-API routes to prevent API 404s from returning index.html
app.MapWhen(
  context =>
    !context.Request.Path.StartsWithSegments("/api") &&
    !context.Request.Path.StartsWithSegments("/health") &&
    !context.Request.Path.StartsWithSegments($"/{DevOnly.ROUTE}"),
  builder => builder.Run(async context =>
  {
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
  }));

app.Run();

public partial class Program
{
}
