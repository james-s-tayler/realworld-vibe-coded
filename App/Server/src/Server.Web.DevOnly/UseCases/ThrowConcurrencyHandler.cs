using Microsoft.EntityFrameworkCore;

namespace Server.Web.DevOnly.UseCases;

public class ThrowConcurrencyHandler : IQueryHandler<ThrowConcurrencyQuery, string>
{
  public Task<Result<string>> Handle(ThrowConcurrencyQuery request, CancellationToken cancellationToken)
  {
    throw new DbUpdateConcurrencyException("Test concurrency conflict");
  }
}
