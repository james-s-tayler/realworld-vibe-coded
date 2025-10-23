using Microsoft.EntityFrameworkCore;

namespace Server.UseCases.ErrorTest;

public class ThrowConcurrencyHandler : IQueryHandler<ThrowConcurrencyQuery, Result<string>>
{
  public Task<Result<string>> Handle(ThrowConcurrencyQuery request, CancellationToken cancellationToken)
  {
    throw new DbUpdateConcurrencyException("Test concurrency conflict");
  }
}
