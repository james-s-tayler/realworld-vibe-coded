using Server.Core.UserAggregate;

namespace Server.UseCases.Profiles.Get;

public record GetProfileQuery(
  string Username,
  Guid? CurrentUserId = null
) : IQuery<User>;
