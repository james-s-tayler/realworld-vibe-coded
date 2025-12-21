using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.Infrastructure;
using Server.Infrastructure.Data;
using Server.Infrastructure.Email;
using Server.UseCases.Interfaces;
using Server.Web.Infrastructure;

namespace Server.Web.Configurations;

public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger, WebApplicationBuilder builder)
  {
    services.AddProblemDetails();

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

    // Configure ASP.NET Identity Core
    services.AddIdentityCore<ApplicationUser>(options =>
    {
      // Password policy
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
    .AddApiEndpoints()
    .AddDefaultTokenProviders();

    // Configure authentication with Identity cookie and bearer token
    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
      options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
      options.DefaultScheme = IdentityConstants.ApplicationScheme;
    })
      .AddCookie(IdentityConstants.ApplicationScheme, options =>
      {
        // Cookie settings
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


    // Configure CSRF protection for cookie-based authentication
    services.AddAntiforgery(options =>
    {
      options.HeaderName = "X-XSRF-TOKEN";
      options.Cookie.Name = "XSRF-TOKEN";
      options.Cookie.HttpOnly = false; // Client needs to read it
      options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
      options.Cookie.SameSite = SameSiteMode.Strict;
    });

    services.AddAuthorization();
    services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationMiddlewareResultHandler,
      Server.Web.Authorization.SuppressBearerChallengeAuthorizationMiddlewareResultHandler>();
    services.AddMemoryCache();
    services.AddHttpContextAccessor();

    // Add custom health check that verifies database migrations are applied
    services.AddHealthChecks()
      .AddCheck<DatabaseMigrationHealthCheck>("database", tags: new[] { "db", "ready" });

    if (builder.Environment.IsDevelopment())
    {
      builder.Services.AddSingleton<IEmailSender, FakeEmailSender>();
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
