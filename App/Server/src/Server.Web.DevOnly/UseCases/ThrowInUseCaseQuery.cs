using Server.SharedKernel.MediatR;

namespace Server.Web.DevOnly.UseCases;

#pragma warning disable SRV015 // DevOnly test endpoint
public record ThrowInUseCaseQuery() : IQuery<Unit>;
#pragma warning restore SRV015
