using Ardalis.Result;
using Ardalis.SharedKernel;
using Microsoft.Extensions.Logging;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;

namespace Server.UseCases.Users.FollowProfile;

public class FollowProfileHandler : ICommandHandler<FollowProfileCommand, Result<ProfileDto>>
{
  private readonly IRepository<User> _repository;
  private readonly ILogger<FollowProfileHandler> _logger;

  public FollowProfileHandler(
    IRepository<User> repository,
    ILogger<FollowProfileHandler> logger)
  {
    _repository = repository;
    _logger = logger;
  }

  public async Task<Result<ProfileDto>> Handle(FollowProfileCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("User {UserId} following {Username}", request.UserId, request.UsernameToFollow);

    // Get the current user
    var currentUser = await _repository.GetByIdAsync(request.UserId, cancellationToken);
    if (currentUser == null)
    {
      return Result.NotFound("Current user not found");
    }

    // Get the user to follow
    var userToFollow = await _repository
      .FirstOrDefaultAsync(new UserByUsernameSpec(request.UsernameToFollow), cancellationToken);
    
    if (userToFollow == null)
    {
      return Result.NotFound("User to follow not found");
    }

    try
    {
      currentUser.Follow(userToFollow);
      await _repository.UpdateAsync(currentUser, cancellationToken);

      _logger.LogInformation("User {UserId} successfully followed {Username}", request.UserId, request.UsernameToFollow);

      return Result.Success(new ProfileDto(
        userToFollow.Username,
        userToFollow.Bio,
        userToFollow.Image,
        true // following is now true
      ));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error following user {Username} by user {UserId}", request.UsernameToFollow, request.UserId);
      return Result.Error("An error occurred while following the user");
    }
  }
}