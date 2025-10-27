using Server.Core.UserAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Profiles.Unfollow;

public record UnfollowUserCommand(
  string Username,
  Guid CurrentUserId
) : ICommand<User>;
