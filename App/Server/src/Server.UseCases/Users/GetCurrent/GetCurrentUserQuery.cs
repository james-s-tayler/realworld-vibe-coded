using Server.SharedKernel.MediatR;
using Server.UseCases.Users.Dtos;

namespace Server.UseCases.Users.GetCurrent;

public record GetCurrentUserQuery(Guid UserId) : IQuery<UserWithRolesDto>;
