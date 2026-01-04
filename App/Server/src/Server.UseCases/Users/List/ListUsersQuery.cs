using Server.SharedKernel.MediatR;
using Server.UseCases.Users.Dtos;

namespace Server.UseCases.Users.List;

public record ListUsersQuery() : IQuery<List<UserWithRolesDto>>;
