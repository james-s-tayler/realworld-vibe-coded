using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Profiles.Unfollow;

public class UnfollowUserHandler(IRepository<User> _userRepository)
  : ICommandHandler<UnfollowUserCommand, User>
{
  public async Task<Result<User>> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
  {
    // Find the user to unfollow
    var userToUnfollow = await _userRepository.FirstOrDefaultAsync(
      new UserByUsernameSpec(request.Username), cancellationToken);

    if (userToUnfollow == null)
    {
      return Result<User>.NotFound(request.Username);
    }

    // Get current user with following relationships
    var currentUser = await _userRepository.FirstOrDefaultAsync(
      new UserWithFollowingSpec(request.CurrentUserId), cancellationToken);

    if (currentUser == null)
    {
      return Result<User>.NotFound(request.CurrentUserId);
    }

    // Check if the user is currently following the target user
    if (!currentUser.IsFollowing(userToUnfollow))
    {
      return Result<User>.Invalid(new ErrorDetail("username", "is not being followed"));
    }

    // Unfollow the user
    currentUser.Unfollow(userToUnfollow);

    return Result<User>.Success(userToUnfollow);
  }
}
