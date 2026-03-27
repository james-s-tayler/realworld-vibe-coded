using Server.SharedKernel.MediatR;

namespace Server.UseCases.Profiles.Follow;

public record FollowCommand(string Username, Guid CurrentUserId) : ICommand<ProfileResult>;
