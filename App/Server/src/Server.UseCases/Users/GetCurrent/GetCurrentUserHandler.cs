using Ardalis.Result;
using Ardalis.SharedKernel;
using Microsoft.Extensions.Logging;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;

namespace Server.UseCases.Users.GetCurrent;

public class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, Result<UserDto>>
{
  private readonly IRepository<User> _repository;
  private readonly IJwtTokenGenerator _jwtTokenGenerator;
  private readonly ILogger<GetCurrentUserHandler> _logger;

  public GetCurrentUserHandler(
    IRepository<User> repository,
    IJwtTokenGenerator jwtTokenGenerator,
    ILogger<GetCurrentUserHandler> logger)
  {
    _repository = repository;
    _jwtTokenGenerator = jwtTokenGenerator;
    _logger = logger;
  }

  public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Getting current user for ID {UserId}", request.UserId);

    var user = await _repository.GetByIdAsync(request.UserId, cancellationToken);

    if (user == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result.NotFound();
    }

    // Generate fresh token
    var token = _jwtTokenGenerator.GenerateToken(user);

    _logger.LogInformation("Retrieved current user {Username}", user.Username);

    return Result.Success(new UserDto(
      user.Id,
      user.Email,
      user.Username,
      user.Bio,
      user.Image,
      token));
  }
}
