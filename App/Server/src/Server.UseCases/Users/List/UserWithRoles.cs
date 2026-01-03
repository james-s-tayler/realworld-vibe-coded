using Server.Core.IdentityAggregate;

namespace Server.UseCases.Users.List;

public record UserWithRoles(ApplicationUser User, List<string> Roles);
