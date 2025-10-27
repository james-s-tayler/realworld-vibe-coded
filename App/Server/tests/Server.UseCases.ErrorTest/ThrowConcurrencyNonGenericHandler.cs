using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Server.UseCases.ErrorTest;

public class ThrowConcurrencyNonGenericHandler : IQueryHandler<ThrowConcurrencyNonGenericQuery, Unit>
{
  public Task<Result<Unit>> Handle(ThrowConcurrencyNonGenericQuery request, CancellationToken cancellationToken)
  {
    throw new DbUpdateConcurrencyException("Test concurrency conflict for non-generic Result");
  }
}
