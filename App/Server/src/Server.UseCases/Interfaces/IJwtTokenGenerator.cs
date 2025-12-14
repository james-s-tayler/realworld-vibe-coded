using Server.Core.IdentityAggregate;

namespace Server.UseCases.Interfaces;

public interface IJwtTokenGenerator
{
  string GenerateToken(ApplicationUser user);
}
