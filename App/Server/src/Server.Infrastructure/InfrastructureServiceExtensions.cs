using Server.Core.Interfaces;
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

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
           .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
           .AddScoped<IUnitOfWork, UnitOfWork>()
           .AddScoped<IListArticlesQueryService, ListArticlesQueryService>()
           .AddScoped<IFeedQueryService, FeedQueryService>()
           .AddScoped<IListTagsQueryService, ListTagsQueryService>()
           .AddScoped<IPasswordHasher, BcryptPasswordHasher>()
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
