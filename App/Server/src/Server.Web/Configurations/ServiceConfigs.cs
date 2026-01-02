using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.Core.TenantInfoAggregate;
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

    services.AddInfrastructureServices(builder, logger)
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
      // Password policy - relaxed to match legacy requirements for backward compatibility
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

    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
      options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
      options.DefaultScheme = IdentityConstants.ApplicationScheme;
    })
    .AddBearerToken(IdentityConstants.BearerScheme)
    .AddCookie(IdentityConstants.ApplicationScheme);

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

    services.AddSingleton<IEmailSender<ApplicationUser>, IdentityEmailSender>();

    services.AddMultiTenant<TenantInfo>()
      .WithClaimStrategy("__tenant__", IdentityConstants.ApplicationScheme)
      .WithClaimStrategy("__tenant__", IdentityConstants.BearerScheme)
      .WithEFCoreStore<TenantStoreDbContext, TenantInfo>();

    logger.LogInformation("{Project} services registered", "Mediatr and Email Sender");

    return services;
  }
}
