using Server.Core.UserAggregate;

namespace Server.UseCases.Profiles.Follow;

public record FollowUserCommand(
  string Username,
  Guid CurrentUserId
) : ICommand<User>;
