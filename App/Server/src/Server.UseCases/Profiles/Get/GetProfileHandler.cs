using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.UseCases.Profiles.Get;

public class GetProfileHandler(IRepository<User> _userRepository)
  : IQueryHandler<GetProfileQuery, Result<User>>
{
  public async Task<Result<User>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.FirstOrDefaultAsync(
      new UserByUsernameWithFollowingSpec(request.Username), cancellationToken);

    if (user == null)
    {
      return Result.NotFound("User not found");
    }

    return Result.Success(user);
  }
}
