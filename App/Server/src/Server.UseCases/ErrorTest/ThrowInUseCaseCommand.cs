namespace Server.UseCases.ErrorTest;

public record ThrowInUseCaseCommand() : ICommand<Result<string>>;
