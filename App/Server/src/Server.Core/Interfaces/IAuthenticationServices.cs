using Server.Core.UserAggregate;

namespace Server.Core.Interfaces;

public interface IPasswordHasher
{
  string HashPassword(User user, string password);
  bool VerifyPassword(User user, string password, string hashedPassword);
}

public interface IJwtTokenGenerator
{
  string GenerateToken(User user);
}
