using Server.Core.UserAggregate;

namespace Server.Core.Interfaces;

public interface IJwtTokenGenerator
{
  string GenerateToken(User user);
}
