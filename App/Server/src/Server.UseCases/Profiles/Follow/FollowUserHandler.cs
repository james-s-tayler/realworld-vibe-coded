using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Profiles.Follow;

public class FollowUserHandler(IRepository<User> _userRepository)
  : ICommandHandler<FollowUserCommand, User>
{
  public async Task<Result<User>> Handle(FollowUserCommand request, CancellationToken cancellationToken)
  {
    // Find the user to follow
    var userToFollow = await _userRepository.FirstOrDefaultAsync(
      new UserByUsernameSpec(request.Username), cancellationToken);

    if (userToFollow == null)
    {
      return Result<User>.NotFound("User not found");
    }

    // Get current user with following relationships
    var currentUser = await _userRepository.FirstOrDefaultAsync(
      new UserWithFollowingSpec(request.CurrentUserId), cancellationToken);

    if (currentUser == null)
    {
      return Result<User>.NotFound("Current user not found");
    }

    // Follow the user
    currentUser.Follow(userToFollow);
    await _userRepository.SaveChangesAsync(cancellationToken);

    return Result<User>.Success(userToFollow);
  }
}
