using Server.Core.IdentityAggregate;

namespace Server.UseCases.Profiles;

public record ProfileResult(ApplicationUser User, bool Following);
