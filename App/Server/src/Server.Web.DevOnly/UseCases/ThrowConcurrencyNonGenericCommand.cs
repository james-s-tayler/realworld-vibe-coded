namespace Server.Web.DevOnly.UseCases;

/// <summary>
/// Command that throws a DbUpdateConcurrencyException for non-generic Result testing
/// </summary>
public record ThrowConcurrencyNonGenericQuery() : IQuery<Unit>;
