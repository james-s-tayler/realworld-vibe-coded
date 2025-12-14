using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Register;

// PV014: This handler uses ASP.NET Identity's UserManager instead of the repository pattern.
// UserManager.CreateAsync performs the database mutation internally.
#pragma warning disable PV014
public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, ApplicationUser>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly ILogger<RegisterUserHandler> _logger;

  public RegisterUserHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<RegisterUserHandler> logger)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _logger = logger;
  }

  public async Task<Result<ApplicationUser>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling user registration for {Email}", request.Email);

    // Check if user already exists by email
    var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
    if (existingUserByEmail != null)
    {
      _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
      return Result<ApplicationUser>.Invalid(new ErrorDetail
      {
        Identifier = nameof(request.Email),
        ErrorMessage = "Email already exists",
      });
    }

    // Check if user already exists by username
    var existingUserByUsername = await _userManager.FindByNameAsync(request.Username);
    if (existingUserByUsername != null)
    {
      _logger.LogWarning("Registration failed: Username {Username} already exists", request.Username);
      return Result<ApplicationUser>.Invalid(new ErrorDetail
      {
        Identifier = nameof(request.Username),
        ErrorMessage = "Username already exists",
      });
    }

    // Create ApplicationUser from request
    var user = new ApplicationUser
    {
      UserName = request.Username,
      Email = request.Email,
      Bio = "I work at statefarm",  // Default bio as per existing User entity
      Image = null,
    };

    // Use UserManager to create user with password
    var result = await _userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
      _logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

      // Map IdentityError to response format
      var errors = result.Errors.Select(e => new ErrorDetail
      {
        Identifier = e.Code.ToLowerInvariant().Contains("password") ? "Password" :
                      e.Code.ToLowerInvariant().Contains("email") ? "Email" :
                      e.Code.ToLowerInvariant().Contains("username") ? "Username" : "body",
        ErrorMessage = e.Description,
      }).ToList();

      return Result<ApplicationUser>.Invalid(errors);
    }

    // Sign in the user to create authentication cookie
    // This allows endpoints to authenticate via cookies in addition to JWT tokens
    await _signInManager.SignInAsync(user, isPersistent: false);

    _logger.LogInformation(
      "User {Username} registered successfully with ID {UserId}",
      user.UserName,
      user.Id);

    return Result<ApplicationUser>.Created(user);
  }
}
#pragma warning restore PV014
