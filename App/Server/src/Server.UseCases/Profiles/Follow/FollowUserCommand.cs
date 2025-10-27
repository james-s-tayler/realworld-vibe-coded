using Server.Core.UserAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Profiles.Follow;

public record FollowUserCommand(
  string Username,
  Guid CurrentUserId
) : ICommand<User>;
