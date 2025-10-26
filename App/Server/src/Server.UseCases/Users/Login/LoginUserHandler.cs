using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.UserAggregate;

namespace Server.UseCases.Users.Login;

public class LoginUserHandler : IQueryHandler<LoginUserQuery, User>
{
  private readonly UserManager<User> _userManager;
  private readonly SignInManager<User> _signInManager;
  private readonly ILogger<LoginUserHandler> _logger;

  public LoginUserHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<LoginUserHandler> logger)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _logger = logger;
  }

  public async Task<Result<User>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling user login for {Email}", request.Email);

    // Find user by email
    var user = await _userManager.FindByEmailAsync(request.Email);

    if (user == null)
    {
      _logger.LogWarning("Login failed: User with email {Email} not found", request.Email);
      return Result.Unauthorized();
    }

    // Check password using SignInManager (this doesn't actually sign in, just checks password)
    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

    if (!result.Succeeded)
    {
      _logger.LogWarning("Login failed: Invalid password for user {Email}", request.Email);
      return Result.Unauthorized();
    }

    _logger.LogInformation("User {Username} logged in successfully", user.Username);

    return Result.Success(user);
  }
}
