namespace Server.UseCases.ErrorTest;

public class ThrowInUseCaseHandler : IQueryHandler<ThrowInUseCaseQuery, string>
{
  public Task<Result<string>> Handle(ThrowInUseCaseQuery request, CancellationToken cancellationToken)
  {
    throw new InvalidOperationException("This is a test exception thrown in the use case");
  }
}
