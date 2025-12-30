using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Server.Infrastructure.Authentication;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Interceptors;
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

    string? tenantStoreConnectionString = config.GetConnectionString("TenantStoreConnection") ?? connectionString;

    // Register the interceptor
    services.AddSingleton<ITimeProvider, UtcNowTimeProvider>();
    services.AddSingleton<AuditableEntityInterceptor>();

    // Configure Finbuckle.MultiTenant with ClaimsStrategy and EFCore store
    services.AddMultiTenant<Server.Core.TenantInfoAggregate.TenantInfo>()
      .WithClaimStrategy()  // Use claims to resolve tenant (default claim type: "__tenant__")
      .WithEFCoreStore<TenantStoreDbContext, Server.Core.TenantInfoAggregate.TenantInfo>();

    // Register TenantStoreDbContext (separate database for tenant information)
    services.AddDbContext<TenantStoreDbContext>(options =>
    {
      options.UseSqlServer(tenantStoreConnectionString);
    });

    services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    {
      options.UseSqlServer(connectionString);
      options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
    });

    // Register repositories for both contexts
    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
           .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
           .AddScoped(typeof(IRepository<Server.Core.TenantInfoAggregate.TenantInfo>), typeof(TenantStoreRepository<Server.Core.TenantInfoAggregate.TenantInfo>))
           .AddScoped(typeof(IReadRepository<Server.Core.TenantInfoAggregate.TenantInfo>), typeof(TenantStoreRepository<Server.Core.TenantInfoAggregate.TenantInfo>))
           .AddScoped<IUnitOfWork, UnitOfWork>()
           .AddScoped<IPasswordHasher, BcryptPasswordHasher>()
           .AddScoped<IUserContext, UserContext>();


    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
