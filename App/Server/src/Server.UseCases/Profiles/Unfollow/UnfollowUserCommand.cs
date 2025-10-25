using Server.Core.UserAggregate;

namespace Server.UseCases.Profiles.Unfollow;

public record UnfollowUserCommand(
  string Username,
  Guid CurrentUserId
) : ICommand<User>;
