using Microsoft.Extensions.Logging;
using Server.Core.UserAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Users.GetCurrent;

public class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, User>
{
  private readonly IRepository<User> _repository;
  private readonly ILogger<GetCurrentUserHandler> _logger;

  public GetCurrentUserHandler(
    IRepository<User> repository,
    ILogger<GetCurrentUserHandler> logger)
  {
    _repository = repository;
    _logger = logger;
  }

  public async Task<Result<User>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Getting current user for ID {UserId}", request.UserId);

    var user = await _repository.GetByIdAsync(request.UserId, cancellationToken);

    if (user == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result.NotFound();
    }

    _logger.LogInformation("Retrieved current user {Username}", user.Username);

    return Result.Success(user);
  }
}
