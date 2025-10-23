namespace Server.UseCases.ErrorTest;

public class ThrowInUseCaseNonGenericHandler : IQueryHandler<ThrowInUseCaseNonGenericQuery, Result>
{
  public Task<Result> Handle(ThrowInUseCaseNonGenericQuery request, CancellationToken cancellationToken)
  {
    throw new InvalidOperationException("Test exception for non-generic Result");
  }
}
