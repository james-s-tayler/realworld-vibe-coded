using Server.SharedKernel.Persistence;

namespace Server.Infrastructure.Data;

/// <summary>
/// Repository for TenantInfo entities stored in the TenantStoreDbContext.
/// </summary>
public class TenantStoreRepository<T>(TenantStoreDbContext dbContext) :
  RepositoryBase<T>(dbContext), IReadRepository<T>, IRepository<T> where T : class, IAggregateRoot
{
}
