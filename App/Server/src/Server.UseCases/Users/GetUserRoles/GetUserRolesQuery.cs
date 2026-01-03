using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.GetUserRoles;

public record GetUserRolesQuery(string UserId) : IQuery<List<string>>;
