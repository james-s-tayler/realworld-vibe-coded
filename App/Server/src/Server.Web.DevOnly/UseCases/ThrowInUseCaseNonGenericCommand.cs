namespace Server.Web.DevOnly.UseCases;

/// <summary>
/// Command that throws an exception for non-generic Result testing
/// </summary>
public record ThrowInUseCaseNonGenericQuery() : IQuery<Unit>;
