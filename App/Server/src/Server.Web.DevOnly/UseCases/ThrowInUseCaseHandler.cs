using Server.SharedKernel.MediatR;

namespace Server.Web.DevOnly.UseCases;

public class ThrowInUseCaseHandler : IQueryHandler<ThrowInUseCaseQuery, Unit>
{
  public Task<Result<Unit>> Handle(ThrowInUseCaseQuery request, CancellationToken cancellationToken)
  {
    throw new InvalidOperationException("This is a test exception thrown in the use case");
  }
}
