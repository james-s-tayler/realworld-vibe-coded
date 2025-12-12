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
// TEMPORARY: During Identity migration, exclude both /api routes and Identity endpoints
app.MapWhen(
  context =>
    !context.Request.Path.StartsWithSegments("/api") &&
    !context.Request.Path.StartsWithSegments("/health") &&
    !context.Request.Path.StartsWithSegments($"/{DevOnly.ROUTE}") &&
    !context.Request.Path.StartsWithSegments("/register") &&
    !context.Request.Path.StartsWithSegments("/login") &&
    !context.Request.Path.StartsWithSegments("/refresh") &&
    !context.Request.Path.StartsWithSegments("/confirmEmail") &&
    !context.Request.Path.StartsWithSegments("/resendConfirmationEmail") &&
    !context.Request.Path.StartsWithSegments("/forgotPassword") &&
    !context.Request.Path.StartsWithSegments("/resetPassword") &&
    !context.Request.Path.StartsWithSegments("/manage"),
  builder => builder.Run(async context =>
  {
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
  }));

app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program
{
}
