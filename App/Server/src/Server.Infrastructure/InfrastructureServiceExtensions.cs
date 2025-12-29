using Finbuckle.MultiTenant.Abstractions;
using Server.Infrastructure.Authentication;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Interceptors;
using Server.Infrastructure.Identity;
using Server.Infrastructure.Services;
using Server.SharedKernel.Interfaces;
using Server.SharedKernel.Persistence;
using Server.UseCases.Identity;
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

    // Configure Finbuckle MultiTenant with Claim strategy
    // This will automatically register IMultiTenantContextAccessor and IMultiTenantContextSetter
    services.AddMultiTenant<TenantInfo>()
      .WithClaimStrategy("TenantId")
      .WithEFCoreStore<AppDbContext, TenantInfo>();

    services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    {
      options.UseSqlServer(connectionString);
      options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
    });

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
           .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
           .AddScoped<IUnitOfWork, UnitOfWork>()
           .AddScoped<IPasswordHasher, BcryptPasswordHasher>()
           .AddScoped<IUserContext, UserContext>()
           .AddScoped<ITenantAssigner, TenantAssigner>()
           .AddScoped<ITenantStore, TenantStore>();


    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
