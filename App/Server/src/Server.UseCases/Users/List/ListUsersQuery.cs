using Server.SharedKernel.MediatR;
using Server.SharedKernel.Pagination;
using Server.UseCases.Users.Dtos;

namespace Server.UseCases.Users.List;

public record ListUsersQuery(int Limit = 20, int Offset = 0) : IQuery<PagedResult<UserWithRolesDto>>;
