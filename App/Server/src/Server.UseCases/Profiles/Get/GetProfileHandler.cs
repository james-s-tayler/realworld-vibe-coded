using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.Core.UserFollowingAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Profiles.Get;

public class GetProfileHandler(
  UserManager<ApplicationUser> userManager,
  IReadRepository<UserFollowing> followingRepo)
  : IQueryHandler<GetProfileQuery, ProfileResult>
{
  public async Task<Result<ProfileResult>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
  {
    var user = await userManager.FindByNameAsync(request.Username);

    if (user == null)
    {
      return Result<ProfileResult>.NotFound(request.Username);
    }

    var isFollowing = false;
    if (request.CurrentUserId.HasValue)
    {
      isFollowing = await followingRepo.AnyAsync(
        new UserFollowingByUsersSpec(request.CurrentUserId.Value, user.Id),
        cancellationToken);
    }

    return Result<ProfileResult>.Success(new ProfileResult(user, isFollowing));
  }
}
