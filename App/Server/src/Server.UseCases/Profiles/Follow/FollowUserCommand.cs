using Server.Core.UserAggregate;

namespace Server.UseCases.Profiles.Follow;

public record FollowUserCommand(
  string Username,
  int CurrentUserId
) : ICommand<Result<User>>;
