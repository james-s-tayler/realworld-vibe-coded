using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Register;

public record RegisterUserCommand(string Email, string Username, string Password) : ICommand<ApplicationUser>;
