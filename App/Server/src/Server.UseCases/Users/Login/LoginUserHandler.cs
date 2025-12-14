using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.Core.UserAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Login;

public class LoginUserHandler : IQueryHandler<LoginUserQuery, User>
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

  public async Task<Result<User>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling user login for {Email}", request.Email);

    // Find user by email
    var user = await _userManager.FindByEmailAsync(request.Email);

    if (user == null)
    {
      _logger.LogWarning("Login failed: User with email {Email} not found", request.Email);
      return Result<User>.Unauthorized(new ErrorDetail
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
      return Result<User>.Unauthorized(new ErrorDetail
      {
        Identifier = "body",
        ErrorMessage = "email or password is invalid",
      });
    }

    _logger.LogInformation("User {Username} logged in successfully", user.UserName);

    // Map ApplicationUser to legacy User for backward compatibility
    var legacyUser = MapToLegacyUser(user);
    return Result<User>.Success(legacyUser);
  }

  private static User MapToLegacyUser(ApplicationUser appUser)
  {
    // Create a User entity with a dummy hashed password since we're using Identity now
    // This is for backward compatibility with code that expects a User entity
    var user = new User(appUser.Email!, appUser.UserName!, "identity-managed")
    {
      Id = appUser.Id,
    };

    user.UpdateBio(appUser.Bio);
    user.UpdateImage(appUser.Image);

    return user;
  }
}
