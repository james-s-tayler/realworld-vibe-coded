using Microsoft.AspNetCore.Identity;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Infrastructure.Authentication;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Interceptors;
using Server.Infrastructure.Data.Queries;
using Server.Infrastructure.Services;
using Server.SharedKernel.Interfaces;


namespace Server.Infrastructure;
public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ConfigurationManager config,
    ILogger logger)
  {
    string? connectionString = config.GetConnectionString("DefaultConnection");
    Guard.Against.Null(connectionString);

    // Register the interceptor
    services.AddSingleton<ITimeProvider, UtcNowTimeProvider>();
    services.AddSingleton<AuditableEntityInterceptor>();

    services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    {
      options.UseSqlServer(connectionString);
      options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
    });

    // Configure ASP.NET Core Identity
    services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
      // Password settings (RealWorld spec doesn't require complex passwords)
      options.Password.RequireDigit = false;
      options.Password.RequireLowercase = false;
      options.Password.RequireNonAlphanumeric = false;
      options.Password.RequireUppercase = false;
      options.Password.RequiredLength = 1;
      options.Password.RequiredUniqueChars = 0;

      // User settings
      options.User.RequireUniqueEmail = true;

      // Sign in settings
      options.SignIn.RequireConfirmedEmail = false;
      options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
           .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
           .AddScoped<IUnitOfWork, UnitOfWork>()
           .AddScoped<IListArticlesQueryService, ListArticlesQueryService>()
           .AddScoped<IFeedQueryService, FeedQueryService>()
           .AddScoped<IListTagsQueryService, ListTagsQueryService>()
           .AddScoped<IPasswordHasher, IdentityPasswordHasher>()
           .AddScoped<IJwtTokenGenerator, JwtTokenGenerator>()
           .AddScoped<ICurrentUserService, CurrentUserService>();

    // Configure JWT settings
    var jwtSettings = new JwtSettings();
    config.GetSection("JwtSettings").Bind(jwtSettings);
    services.AddSingleton(jwtSettings);


    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
