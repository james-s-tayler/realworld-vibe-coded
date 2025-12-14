using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Login;

public class LoginUserHandler : IQueryHandler<LoginUserQuery, ApplicationUser>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly ILogger<LoginUserHandler> _logger;

  public LoginUserHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<LoginUserHandler> logger)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _logger = logger;
  }

  public async Task<Result<ApplicationUser>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling user login for {Email}", request.Email);

    // Find user by email
    var user = await _userManager.FindByEmailAsync(request.Email);

    if (user == null)
    {
      _logger.LogWarning("Login failed: User with email {Email} not found", request.Email);
      return Result<ApplicationUser>.Unauthorized(new ErrorDetail
      {
        Identifier = "body",
        ErrorMessage = "email or password is invalid",
      });
    }

    // Verify password using SignInManager
    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

    if (!result.Succeeded)
    {
      _logger.LogWarning("Login failed: Invalid password for user {Email}", request.Email);
      return Result<ApplicationUser>.Unauthorized(new ErrorDetail
      {
        Identifier = "body",
        ErrorMessage = "email or password is invalid",
      });
    }

    _logger.LogInformation("User {Username} logged in successfully", user.UserName);

    return Result<ApplicationUser>.Success(user);
  }
}
