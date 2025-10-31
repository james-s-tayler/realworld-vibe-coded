using Microsoft.EntityFrameworkCore;
using Server.SharedKernel.MediatR;

namespace Server.Web.DevOnly.UseCases;

#pragma warning disable SRV015 // DevOnly test endpoint
public class ThrowConcurrencyHandler : IQueryHandler<ThrowConcurrencyQuery, Unit>
{
  public Task<Result<Unit>> Handle(ThrowConcurrencyQuery request, CancellationToken cancellationToken)
  {
    throw new DbUpdateConcurrencyException("Test concurrency conflict");
  }
}
#pragma warning restore SRV015
