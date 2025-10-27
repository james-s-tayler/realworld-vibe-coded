using Microsoft.EntityFrameworkCore;
using Server.SharedKernel.MediatR;

namespace Server.Web.DevOnly.UseCases;

public class ThrowConcurrencyHandler : IQueryHandler<ThrowConcurrencyQuery, Unit>
{
  public Task<Result<Unit>> Handle(ThrowConcurrencyQuery request, CancellationToken cancellationToken)
  {
    throw new DbUpdateConcurrencyException("Test concurrency conflict");
  }
}
