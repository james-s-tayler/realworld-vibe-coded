using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Profiles.Unfollow;

public record UnfollowUserCommand(
  string Username,
  Guid CurrentUserId
) : ICommand<ApplicationUser>;
