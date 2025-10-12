using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Server.Core.Interfaces;
using Server.Infrastructure;
using Server.Infrastructure.Authentication;
using Server.Infrastructure.Email;
using Server.Web.Infrastructure;

namespace Server.Web.Configurations;

public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger, WebApplicationBuilder builder)
  {
    services.AddInfrastructureServices(builder.Configuration, logger)
            .AddMediatrConfigs();

    // Configure CORS for local development
    services.AddCors(options =>
    {
      options.AddPolicy("AllowLocalhost", policy =>
      {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
      });
    });

    // Configure JWT Authentication
    var jwtSettings = new JwtSettings();
    builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);

    services.AddAuthentication("Token")
      .AddJwtBearer("Token", options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
          ValidateIssuer = true,
          ValidIssuer = jwtSettings.Issuer,
          ValidateAudience = true,
          ValidAudience = jwtSettings.Audience,
          ValidateLifetime = true,
          ClockSkew = TimeSpan.Zero
        };

        // Configure events to return JSON error responses for authentication failures
        options.Events = new JwtBearerEvents
        {
          OnMessageReceived = context =>
          {
            string? authorization = context.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Token ", StringComparison.OrdinalIgnoreCase))
            {
              context.Token = authorization.Substring("Token ".Length).Trim();
            }

            return Task.CompletedTask;
          },
          OnChallenge = context =>
          {
            // Skip the default logic that adds WWW-Authenticate header
            context.HandleResponse();

            var errorResponse = new ConduitErrorResponse
            {
              Errors = new ConduitErrorBody { Body = new[] { "Unauthorized" } }
            };

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsJsonAsync(errorResponse);
          },
          OnForbidden = context =>
          {
            var errorResponse = new ConduitErrorResponse
            {
              Errors = new ConduitErrorBody { Body = new[] { "Unauthorized" } }
            };

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsJsonAsync(errorResponse);
          }
        };
      });

    services.AddAuthorization();

    // Register global exception handler for unauthorized access
    services.AddExceptionHandler<UnauthorizedExceptionHandler>();
    services.AddProblemDetails();

    // Register IHttpContextAccessor for CurrentUserService
    services.AddHttpContextAccessor();

    if (builder.Environment.IsDevelopment())
    {
      // Use a local test email server
      // See: https://ardalis.com/configuring-a-local-test-email-server/
      services.AddScoped<IEmailSender, MimeKitEmailSender>();

      // Otherwise use this:
      //builder.Services.AddScoped<IEmailSender, FakeEmailSender>();

    }
    else
    {
      services.AddScoped<IEmailSender, MimeKitEmailSender>();
    }

    logger.LogInformation("{Project} services registered", "Mediatr and Email Sender");

    return services;
  }


}
