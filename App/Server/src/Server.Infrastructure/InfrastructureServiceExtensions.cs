using Server.Core.ArticleAggregate;
using Server.Core.TagAggregate;
using Server.Infrastructure.Authentication;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Interceptors;
using Server.Infrastructure.Data.Queries;
using Server.Infrastructure.Services;
using Server.SharedKernel.Interfaces;
using Server.SharedKernel.Persistence;
using Server.UseCases.Interfaces;


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
           .AddScoped<IFeedQueryService, FeedQueryService>()
           .AddScoped<IListTagsQueryService, ListTagsQueryService>()
           .AddScoped<IPasswordHasher, BcryptPasswordHasher>()
           .AddScoped<IJwtTokenGenerator, JwtTokenGenerator>()
           .AddScoped<IUserContext, UserContext>();

    // Configure JWT settings
    var jwtSettings = new JwtSettings();
    config.GetSection("JwtSettings").Bind(jwtSettings);
    services.AddSingleton(jwtSettings);


    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
