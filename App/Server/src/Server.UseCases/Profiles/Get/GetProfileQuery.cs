using Server.Core.UserAggregate;

namespace Server.UseCases.Profiles.Get;

public record GetProfileQuery(
  string Username,
  int? CurrentUserId = null
) : IQuery<Result<User>>;
