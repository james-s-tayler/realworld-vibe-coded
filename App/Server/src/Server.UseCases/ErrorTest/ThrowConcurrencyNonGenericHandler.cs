using Microsoft.EntityFrameworkCore;

namespace Server.UseCases.ErrorTest;

public class ThrowConcurrencyNonGenericHandler : IQueryHandler<ThrowConcurrencyNonGenericQuery, Result>
{
  public Task<Result> Handle(ThrowConcurrencyNonGenericQuery request, CancellationToken cancellationToken)
  {
    throw new DbUpdateConcurrencyException("Test concurrency conflict for non-generic Result");
  }
}
