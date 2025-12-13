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
// TEMPORARY: During Identity migration, exclude Identity API endpoints but only for non-GET requests
// This allows the SPA to serve the React app for GET /login etc. while allowing POST /login for Identity API
app.MapWhen(
  context =>
  {
    var path = context.Request.Path;
    var method = context.Request.Method;

    // Never serve SPA for these paths
    if (path.StartsWithSegments("/api") ||
        path.StartsWithSegments("/health") ||
        path.StartsWithSegments($"/{DevOnly.ROUTE}"))
    {
      return false;
    }

    // For Identity endpoints, only exclude non-GET requests (Identity API uses POST)
    // This allows GET requests to serve the SPA for React routes like /login
    if (method != "GET" && (
        path.StartsWithSegments("/register") ||
        path.StartsWithSegments("/login") ||
        path.StartsWithSegments("/refresh") ||
        path.StartsWithSegments("/confirmEmail") ||
        path.StartsWithSegments("/resendConfirmationEmail") ||
        path.StartsWithSegments("/forgotPassword") ||
        path.StartsWithSegments("/resetPassword") ||
        path.StartsWithSegments("/manage")))
    {
      return false;
    }

    return true;
  },
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
