using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.Core.UserAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Register;

// PV014: This handler uses ASP.NET Identity's UserManager instead of the repository pattern.
// UserManager.CreateAsync performs the database mutation internally.
#pragma warning disable PV014
public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, User>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<RegisterUserHandler> _logger;

  public RegisterUserHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<RegisterUserHandler> logger)
  {
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<Result<User>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling user registration for {Email}", request.Email);

    // Check if user already exists by email
    var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
    if (existingUserByEmail != null)
    {
      _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
      return Result<User>.Invalid(new ErrorDetail
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
      return Result<User>.Invalid(new ErrorDetail
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

      return Result<User>.Invalid(errors);
    }

    _logger.LogInformation(
      "User {Username} registered successfully with ID {UserId}",
      user.UserName,
      user.Id);

    // Map ApplicationUser to legacy User for backward compatibility
    var legacyUser = MapToLegacyUser(user);
    return Result<User>.Created(legacyUser);
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
#pragma warning restore PV014
