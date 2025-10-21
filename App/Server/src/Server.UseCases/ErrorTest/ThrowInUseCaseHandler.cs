namespace Server.UseCases.ErrorTest;

public class ThrowInUseCaseHandler : ICommandHandler<ThrowInUseCaseCommand, Result<string>>
{
  public Task<Result<string>> Handle(ThrowInUseCaseCommand request, CancellationToken cancellationToken)
  {
    throw new InvalidOperationException("This is a test exception thrown in the use case");
  }
}
