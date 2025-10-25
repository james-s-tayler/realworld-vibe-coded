using Microsoft.AspNetCore.Identity;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;

namespace Server.Infrastructure.Authentication;

public class IdentityPasswordHasher : IPasswordHasher
{
  private readonly PasswordHasher<User> _passwordHasher;

  public IdentityPasswordHasher()
  {
    _passwordHasher = new PasswordHasher<User>();
  }

  public string HashPassword(User user, string password)
  {
    return _passwordHasher.HashPassword(user, password);
  }

  public bool VerifyPassword(User user, string password, string hashedPassword)
  {
    var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
    return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
  }
}
