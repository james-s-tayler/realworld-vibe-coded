using Server.Core.UserAggregate;

namespace Server.Infrastructure.Data;

/// <summary>
/// Repository that routes to the appropriate DbContext based on entity type.
/// User entity uses IdentityDbContext, all other entities use DomainDbContext.
/// </summary>
public class ContextAwareRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T>
  where T : class, IAggregateRoot
{
  public ContextAwareRepository(IdentityDbContext identityDbContext, DomainDbContext domainDbContext)
    : base(GetAppropriateContext<T>(identityDbContext, domainDbContext))
  {
  }

  private static DbContext GetAppropriateContext<TEntity>(
    IdentityDbContext identityDbContext,
    DomainDbContext domainDbContext)
  {
    // User entity uses IdentityDbContext
    if (typeof(TEntity) == typeof(User))
    {
      return identityDbContext;
    }

    // All other entities use DomainDbContext
    return domainDbContext;
  }
}
