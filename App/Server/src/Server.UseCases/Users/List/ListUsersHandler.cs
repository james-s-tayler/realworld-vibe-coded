using Server.SharedKernel.MediatR;
using Server.SharedKernel.Pagination;
using Server.UseCases.Interfaces;
using Server.UseCases.Users.Dtos;

namespace Server.UseCases.Users.List;

public class ListUsersHandler : IQueryHandler<ListUsersQuery, PagedResult<UserWithRolesDto>>
{
  private readonly IQueryApplicationUsers _queryApplicationUsers;

  public ListUsersHandler(IQueryApplicationUsers queryApplicationUsers)
  {
    _queryApplicationUsers = queryApplicationUsers;
  }

  public async Task<Result<PagedResult<UserWithRolesDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
  {
    var users = await _queryApplicationUsers.ListUsersWithRoles(request.Limit, request.Offset, cancellationToken);
    var totalCount = await _queryApplicationUsers.CountUsers(cancellationToken);
    return Result<PagedResult<UserWithRolesDto>>.Success(new PagedResult<UserWithRolesDto>(users, totalCount));
  }
}
