using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Reactivate;

public record ReactivateUserCommand(Guid UserId) : ICommand<ApplicationUser>;
