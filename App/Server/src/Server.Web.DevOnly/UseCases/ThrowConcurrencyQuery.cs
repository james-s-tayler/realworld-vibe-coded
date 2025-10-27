using Server.SharedKernel.MediatR;

namespace Server.Web.DevOnly.UseCases;

/// <summary>
/// Command that throws a DbUpdateConcurrencyException to test conflict handling
/// </summary>
public record ThrowConcurrencyQuery() : IQuery<Unit>;
