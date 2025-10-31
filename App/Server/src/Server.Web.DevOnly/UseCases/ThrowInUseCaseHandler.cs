using Server.SharedKernel.MediatR;

namespace Server.Web.DevOnly.UseCases;

#pragma warning disable SRV015 // DevOnly test endpoint
public class ThrowInUseCaseHandler : IQueryHandler<ThrowInUseCaseQuery, Unit>
{
  public Task<Result<Unit>> Handle(ThrowInUseCaseQuery request, CancellationToken cancellationToken)
  {
    throw new InvalidOperationException("This is a test exception thrown in the use case");
  }
}
#pragma warning restore SRV015
