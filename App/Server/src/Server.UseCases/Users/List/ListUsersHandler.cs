using Server.SharedKernel.MediatR;
using Server.UseCases.Interfaces;

namespace Server.UseCases.Users.List;

public class ListUsersHandler : IQueryHandler<ListUsersQuery, ListUsersResult>
{
  private readonly IQueryApplicationUsers _queryApplicationUsers;

  public ListUsersHandler(IQueryApplicationUsers queryApplicationUsers)
  {
    _queryApplicationUsers = queryApplicationUsers;
  }

  public async Task<Result<ListUsersResult>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
  {
    var users = await _queryApplicationUsers.ListUsersWithRoles(request.Limit, request.Offset, cancellationToken);
    var totalCount = await _queryApplicationUsers.CountUsers(cancellationToken);
    return Result<ListUsersResult>.Success(new ListUsersResult(users, totalCount));
  }
}
