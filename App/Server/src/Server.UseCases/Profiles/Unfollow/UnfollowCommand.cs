using Server.SharedKernel.MediatR;

namespace Server.UseCases.Profiles.Unfollow;

public record UnfollowCommand(string Username, Guid CurrentUserId) : ICommand<ProfileResult>;
