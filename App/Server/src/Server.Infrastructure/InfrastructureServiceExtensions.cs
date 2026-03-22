using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
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
    WebApplicationBuilder builder,
    ILogger logger)
  {
    string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Guard.Against.Null(connectionString);

    // Register the interceptor
    services.AddSingleton<ITimeProvider, UtcNowTimeProvider>();
    services.AddSingleton<AuditableEntityInterceptor>();

    services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    {
      options.UseSqlServer(connectionString);
      options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());

      if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
      {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
      }
    });

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
           .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
           .AddScoped<IUnitOfWork, UnitOfWork>()
           .AddScoped<IPasswordHasher, BcryptPasswordHasher>()
           .AddScoped<IUserContext, UserContext>()
           .AddScoped<IQueryApplicationUsers, ApplicationUsersQuery>();


    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
