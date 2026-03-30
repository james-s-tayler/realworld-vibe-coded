using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Deactivate;

public record DeactivateUserCommand(Guid UserId, Guid CurrentUserId) : ICommand<ApplicationUser>;
