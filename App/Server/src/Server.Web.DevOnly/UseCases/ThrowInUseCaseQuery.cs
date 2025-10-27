using Server.SharedKernel.MediatR;

namespace Server.Web.DevOnly.UseCases;

public record ThrowInUseCaseQuery() : IQuery<Unit>;
