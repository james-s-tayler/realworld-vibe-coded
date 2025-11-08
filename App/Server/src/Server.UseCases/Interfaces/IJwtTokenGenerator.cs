using Server.Core.UserAggregate;

namespace Server.UseCases.Interfaces;

public interface IJwtTokenGenerator
{
  string GenerateToken(User user);
}
