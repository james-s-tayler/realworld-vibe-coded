using MediatR;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Identity.Invite;

public record InviteCommand(string Email, string Password) : ICommand<Unit>;
