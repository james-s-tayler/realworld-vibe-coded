using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.List;

public record ListUsersQuery() : IQuery<List<UserWithRoles>>;
