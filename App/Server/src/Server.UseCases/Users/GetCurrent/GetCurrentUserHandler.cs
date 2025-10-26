using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.UserAggregate;

namespace Server.UseCases.Users.GetCurrent;

public class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, User>
{
  private readonly UserManager<User> _userManager;
  private readonly ILogger<GetCurrentUserHandler> _logger;

  public GetCurrentUserHandler(
    UserManager<User> userManager,
    ILogger<GetCurrentUserHandler> logger)
  {
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<Result<User>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Getting current user for ID {UserId}", request.UserId);

    var user = await _userManager.FindByIdAsync(request.UserId.ToString());

    if (user == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result.NotFound();
    }

    _logger.LogInformation("Retrieved current user {Username}", user.Username);

    return Result.Success(user);
  }
}
