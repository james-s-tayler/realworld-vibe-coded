using Microsoft.Extensions.Logging;
using Server.SharedKernel.MediatR;
using Server.UseCases.Interfaces;
using Server.UseCases.Users.Dtos;

namespace Server.UseCases.Users.GetCurrent;

public class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, UserWithRolesDto>
{
  private readonly IQueryApplicationUsers _queryApplicationUsers;
  private readonly ILogger<GetCurrentUserHandler> _logger;

  public GetCurrentUserHandler(
    IQueryApplicationUsers queryApplicationUsers,
    ILogger<GetCurrentUserHandler> logger)
  {
    _queryApplicationUsers = queryApplicationUsers;
    _logger = logger;
  }

  public async Task<Result<UserWithRolesDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Getting current user for ID {UserId}", request.UserId);

    var userDto = await _queryApplicationUsers.GetCurrentUserWithRoles(request.UserId, cancellationToken);

    if (userDto == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result<UserWithRolesDto>.NotFound();
    }

    _logger.LogInformation("Retrieved current user {Username}", userDto.Username);

    return Result<UserWithRolesDto>.Success(userDto);
  }
}
