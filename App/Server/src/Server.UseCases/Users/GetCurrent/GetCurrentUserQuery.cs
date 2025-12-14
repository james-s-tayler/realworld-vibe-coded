using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.GetCurrent;

public record GetCurrentUserQuery(Guid UserId) : IQuery<ApplicationUser>;
