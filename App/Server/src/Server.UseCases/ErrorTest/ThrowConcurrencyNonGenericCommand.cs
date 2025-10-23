using MediatR;

namespace Server.UseCases.ErrorTest;

/// <summary>
/// Command that throws a DbUpdateConcurrencyException for non-generic Result testing
/// </summary>
public record ThrowConcurrencyNonGenericQuery() : IQuery<Result<Unit>>;
