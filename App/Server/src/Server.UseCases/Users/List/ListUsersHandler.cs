using Server.SharedKernel.MediatR;
using Server.UseCases.Interfaces;
using Server.UseCases.Users.Dtos;

namespace Server.UseCases.Users.List;

public class ListUsersHandler : IQueryHandler<ListUsersQuery, List<UserWithRolesDto>>
{
  private readonly IQueryApplicationUsers _queryApplicationUsers;

  public ListUsersHandler(IQueryApplicationUsers queryApplicationUsers)
  {
    _queryApplicationUsers = queryApplicationUsers;
  }

  public async Task<Result<List<UserWithRolesDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
  {
    var users = await _queryApplicationUsers.ListUsersWithRoles(cancellationToken);
    return Result<List<UserWithRolesDto>>.Success(users);
  }
}
