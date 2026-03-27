using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.Core.UserFollowingAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Profiles.Unfollow;

public class UnfollowHandler(
  UserManager<ApplicationUser> userManager,
  IRepository<UserFollowing> repository)
  : ICommandHandler<UnfollowCommand, ProfileResult>
{
  public async Task<Result<ProfileResult>> Handle(UnfollowCommand request, CancellationToken cancellationToken)
  {
    var user = await userManager.FindByNameAsync(request.Username);

    if (user == null)
    {
      return Result<ProfileResult>.NotFound(request.Username);
    }

    var existing = await repository.FirstOrDefaultAsync(
      new UserFollowingByUsersSpec(request.CurrentUserId, user.Id),
      cancellationToken);

    if (existing == null)
    {
      return Result<ProfileResult>.Invalid(
        new ErrorDetail("username", "You are not following this user."));
    }

    await repository.DeleteAsync(existing, cancellationToken);

    return Result<ProfileResult>.Success(new ProfileResult(user, false));
  }
}
