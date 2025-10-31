using Server.SharedKernel.MediatR;

namespace Server.Web.DevOnly.UseCases;

/// <summary>
/// Command that throws a DbUpdateConcurrencyException to test conflict handling
/// </summary>
#pragma warning disable SRV015 // DevOnly test endpoint
public record ThrowConcurrencyQuery() : IQuery<Unit>;
#pragma warning restore SRV015
