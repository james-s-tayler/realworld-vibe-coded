using MediatR;

namespace Server.UseCases.ErrorTest;

/// <summary>
/// Command that throws an exception for non-generic Result testing
/// </summary>
public record ThrowInUseCaseNonGenericQuery() : IQuery<Result<Unit>>;
