using Microsoft.Extensions.Logging;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;

namespace Server.UseCases.Users.Login;

public class LoginUserHandler : IQueryHandler<LoginUserQuery, User>
{
  private readonly IRepository<User> _repository;
  private readonly IPasswordHasher _passwordHasher;
  private readonly ILogger<LoginUserHandler> _logger;

  public LoginUserHandler(
    IRepository<User> repository,
    IPasswordHasher passwordHasher,
    ILogger<LoginUserHandler> logger)
  {
    _repository = repository;
    _passwordHasher = passwordHasher;
    _logger = logger;
  }

  public async Task<Result<User>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling user login for {Email}", request.Email);

    // Find user by email
    var user = await _repository
      .FirstOrDefaultAsync(new UserByEmailSpec(request.Email), cancellationToken);

    if (user == null)
    {
      _logger.LogWarning("Login failed: User with email {Email} not found", request.Email);
      return Result.Unauthorized();
    }

    // Verify password using Identity's password hasher
    if (!_passwordHasher.VerifyPassword(user, request.Password, user.HashedPassword))
    {
      _logger.LogWarning("Login failed: Invalid password for user {Email}", request.Email);
      return Result.Unauthorized();
    }

    _logger.LogInformation("User {Username} logged in successfully", user.Username);

    return Result.Success(user);
  }
}
