using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Server.Core.IdentityAggregate;
using Server.Infrastructure;
using Server.Infrastructure.Authentication;
using Server.Infrastructure.Data;
using Server.Infrastructure.Email;
using Server.UseCases.Interfaces;
using Server.Web.Infrastructure;
using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace Server.Web.Configurations;

public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger, WebApplicationBuilder builder)
  {
    services.AddInfrastructureServices(builder.Configuration, logger)
            .AddMediatrConfigs();

    // Configure CORS to allow any origin
    services.AddCors(options =>
    {
      options.AddPolicy("AllowLocalhost", policy =>
      {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
      });
    });

    // Configure JWT Authentication
    var jwtSettings = new JwtSettings();
    builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);

    // Configure authentication with Token (JWT) as default scheme for APIs
    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = "Token";
      options.DefaultChallengeScheme = "Token";
      options.DefaultScheme = "Token";
    })
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
          ClockSkew = TimeSpan.Zero,
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
          OnChallenge = async context =>
          {
            context.HandleResponse();
            await context.HttpContext.Response.SendErrorsAsync(
              new List<ValidationFailure>([
                new ValidationFailure(
                  context.Error ?? "authorization",
                  context.ErrorDescription ?? "Unauthorized"),
              ]),
              StatusCodes.Status401Unauthorized);
          },
          OnForbidden = async context =>
          {
            await context.HttpContext.Response.SendErrorsAsync(
              new List<ValidationFailure>([
                new ValidationFailure(
                  "authorization",
                  context.Result?.Failure?.Message ?? "Forbidden"),
              ]),
              StatusCodes.Status403Forbidden);
          },
        };
      });

    // Configure ASP.NET Identity Core (without authentication defaults to avoid conflicts with JWT)
    services.AddIdentityCore<ApplicationUser>(options =>
    {
      // Password policy - relaxed to match legacy requirements for backward compatibility
      // TODO: Tighten these requirements after migrating all tests and clients
      options.Password.RequireDigit = false;
      options.Password.RequireLowercase = false;
      options.Password.RequireUppercase = false;
      options.Password.RequireNonAlphanumeric = false;
      options.Password.RequiredLength = 6;

      // Lockout policy
      options.Lockout.MaxFailedAccessAttempts = 5;
      options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
      options.Lockout.AllowedForNewUsers = true;

      // User options
      options.User.RequireUniqueEmail = true;

      // Sign-in options (no email confirmation for now)
      options.SignIn.RequireConfirmedEmail = false;
      options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager<SignInManager<ApplicationUser>>()
    .AddDefaultTokenProviders();

    // Add cookie authentication for Identity (without setting as default scheme)
    services.AddAuthentication()
      .AddCookie(IdentityConstants.ApplicationScheme, options =>
      {
        // Cookie settings (Decision 4: SameSite.Lax)
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = "ConduitAuth";

        // Expiration and sliding
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        // API-friendly: Return 401/403 instead of redirecting
        options.Events.OnRedirectToLogin = context =>
        {
          if (!context.Response.HasStarted)
          {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
          }

          return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
          if (!context.Response.HasStarted)
          {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
          }

          return Task.CompletedTask;
        };
      })
      .AddBearerToken(IdentityConstants.BearerScheme);

    services.AddAuthorization();
    services.AddProblemDetails();
    services.AddMemoryCache();
    services.AddHttpContextAccessor();

    // Add custom health check that verifies database migrations are applied
    services.AddHealthChecks()
      .AddCheck<DatabaseMigrationHealthCheck>("database", tags: new[] { "db", "ready" });

    if (builder.Environment.IsDevelopment())
    {
      // Use a local test email server
      // See: https://ardalis.com/configuring-a-local-test-email-server/
      services.AddSingleton<IEmailSender, MimeKitEmailSender>();

      // Otherwise use this:
      // builder.Services.AddSingleton<IEmailSender, FakeEmailSender>();
    }
    else
    {
      services.AddSingleton<IEmailSender, MimeKitEmailSender>();
    }

    services.AddSingleton<Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>, Server.Infrastructure.Email.IdentityEmailSender>();

    logger.LogInformation("{Project} services registered", "Mediatr and Email Sender");

    return services;
  }
}
