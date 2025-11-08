using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Profiles.Follow;

public class FollowUserHandler(IRepository<User> userRepository)
  : ICommandHandler<FollowUserCommand, User>
{
  public async Task<Result<User>> Handle(FollowUserCommand request, CancellationToken cancellationToken)
  {
    // Find the user to follow
    var userToFollow = await userRepository.FirstOrDefaultAsync(
      new UserByUsernameSpec(request.Username), cancellationToken);

    if (userToFollow == null)
    {
      return Result<User>.NotFound(request.Username);
    }

    // Get current user with following relationships
    var currentUser = await userRepository.FirstOrDefaultAsync(
      new UserWithFollowingSpec(request.CurrentUserId), cancellationToken);

    if (currentUser == null)
    {
      return Result<User>.NotFound(request.CurrentUserId);
    }

    // Follow the user
    currentUser.Follow(userToFollow);
    await userRepository.UpdateAsync(currentUser, cancellationToken);

    return Result<User>.Success(userToFollow);
  }
}
