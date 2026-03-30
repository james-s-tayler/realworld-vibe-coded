using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.UpdateRoles;

public record UpdateRolesCommand(Guid UserId, Guid CurrentUserId, List<string> Roles) : ICommand<ApplicationUser>;
