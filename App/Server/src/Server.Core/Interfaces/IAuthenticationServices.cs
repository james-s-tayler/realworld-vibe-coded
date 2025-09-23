using Server.Core.UserAggregate;

namespace Server.Core.Interfaces;

public interface IPasswordHasher
{
  string HashPassword(string password);
  bool VerifyPassword(string password, string hashedPassword);
}

public interface IJwtTokenGenerator
{
  string GenerateToken(User user);
}
