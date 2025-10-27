namespace Server.Web.ErrorTestEndpoints.UseCases;

/// <summary>
/// Command that throws a DbUpdateConcurrencyException to test conflict handling
/// </summary>
public record ThrowConcurrencyQuery() : IQuery<string>;
