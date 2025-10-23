namespace Server.UseCases.ErrorTest;

public class ThrowInUseCaseNonGenericHandler : IQueryHandler<ThrowInUseCaseNonGenericQuery, Unit>
{
  public Task<Result<Unit>> Handle(ThrowInUseCaseNonGenericQuery request, CancellationToken cancellationToken)
  {
    throw new InvalidOperationException("Test exception for non-generic Result");
  }
}
