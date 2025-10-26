using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.UserAggregate;

namespace Server.UseCases.Users.Register;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, User>
{
  private readonly UserManager<User> _userManager;
  private readonly ILogger<RegisterUserHandler> _logger;

  public RegisterUserHandler(
    UserManager<User> userManager,
    ILogger<RegisterUserHandler> logger)
  {
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<Result<User>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling user registration for {Email}", request.Email);

    // Create user without password (UserManager will handle password hashing)
    var newUser = new User(request.Email, request.Username);

    // Use UserManager to create user with password
    var result = await _userManager.CreateAsync(newUser, request.Password);

    if (!result.Succeeded)
    {
      _logger.LogWarning("Registration failed for {Email}: {Errors}",
        request.Email,
        string.Join(", ", result.Errors.Select(e => e.Description)));

      // Map Identity errors to validation errors
      var errors = result.Errors.Select(e => new ValidationError
      {
        Identifier = e.Code.Contains("Email") ? nameof(request.Email) :
                    e.Code.Contains("UserName") || e.Code.Contains("Username") ? nameof(request.Username) :
                    e.Code.Contains("Password") ? nameof(request.Password) :
                    "error",
        ErrorMessage = e.Description
      }).ToList();

      return Result.Invalid(errors);
    }

    _logger.LogInformation("User {Username} registered successfully with ID {UserId}",
      newUser.Username, newUser.Id);

    return Result.Created(newUser);
  }
}
