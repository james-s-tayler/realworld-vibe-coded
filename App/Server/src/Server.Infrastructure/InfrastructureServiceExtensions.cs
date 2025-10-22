using Server.Core.Interfaces;
using Server.Infrastructure.Authentication;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Queries;
using Server.Infrastructure.Services;


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
    services.AddDbContext<AppDbContext>(options =>
     options.UseSqlServer(connectionString));

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
