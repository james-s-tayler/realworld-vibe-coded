using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.Core.UserFollowingAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Profiles.Follow;

public class FollowHandler(
  UserManager<ApplicationUser> userManager,
  IRepository<UserFollowing> repository)
  : ICommandHandler<FollowCommand, ProfileResult>
{
  public async Task<Result<ProfileResult>> Handle(FollowCommand request, CancellationToken cancellationToken)
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
      var following = new UserFollowing
      {
        FollowerId = request.CurrentUserId,
        FollowedId = user.Id,
      };
      await repository.AddAsync(following, cancellationToken);
    }

    return Result<ProfileResult>.Success(new ProfileResult(user, true));
  }
}
