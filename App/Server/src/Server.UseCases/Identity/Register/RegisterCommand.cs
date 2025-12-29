using MediatR;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Identity.Register;

public record RegisterCommand(string Email, string Password) : ICommand<Unit>;
