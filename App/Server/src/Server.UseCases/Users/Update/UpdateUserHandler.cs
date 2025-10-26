using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.UserAggregate;

namespace Server.UseCases.Users.Update;

public class UpdateUserHandler : ICommandHandler<UpdateUserCommand, User>
{
  private readonly UserManager<User> _userManager;
  private readonly ILogger<UpdateUserHandler> _logger;

  public UpdateUserHandler(
    UserManager<User> userManager,
    ILogger<UpdateUserHandler> logger)
  {
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<Result<User>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Updating user {UserId}", request.UserId);

    var user = await _userManager.FindByIdAsync(request.UserId.ToString());

    if (user == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result.NotFound();
    }

    // Update email if provided
    if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
    {
      // Check if email is already taken
      var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
      if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
      {
        _logger.LogWarning("Update failed: Email {Email} already exists", request.Email);
        return Result.Invalid(new ValidationError
        {
          Identifier = "email",
          ErrorMessage = "Email already exists",
        });
      }

      var emailResult = await _userManager.SetEmailAsync(user, request.Email);
      if (!emailResult.Succeeded)
      {
        _logger.LogWarning("Update failed: Could not update email {Email}", request.Email);
        return Result.Invalid(emailResult.Errors.Select(e => new ValidationError
        {
          Identifier = "email",
          ErrorMessage = e.Description
        }).ToList());
      }
    }

    // Update username if provided
    if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
    {
      // Check if username is already taken
      var existingUserByUsername = await _userManager.FindByNameAsync(request.Username);
      if (existingUserByUsername != null && existingUserByUsername.Id != user.Id)
      {
        _logger.LogWarning("Update failed: Username {Username} already exists", request.Username);
        return Result.Invalid(new ValidationError
        {
          Identifier = "username",
          ErrorMessage = "Username already exists",
        });
      }

      var usernameResult = await _userManager.SetUserNameAsync(user, request.Username);
      if (!usernameResult.Succeeded)
      {
        _logger.LogWarning("Update failed: Could not update username {Username}", request.Username);
        return Result.Invalid(usernameResult.Errors.Select(e => new ValidationError
        {
          Identifier = "username",
          ErrorMessage = e.Description
        }).ToList());
      }
    }

    // Update password if provided
    if (!string.IsNullOrEmpty(request.Password))
    {
      // Remove old password and add new one
      var token = await _userManager.GeneratePasswordResetTokenAsync(user);
      var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.Password);

      if (!passwordResult.Succeeded)
      {
        _logger.LogWarning("Update failed: Could not update password");
        return Result.Invalid(passwordResult.Errors.Select(e => new ValidationError
        {
          Identifier = "password",
          ErrorMessage = e.Description
        }).ToList());
      }
    }

    // Update bio if provided
    if (request.Bio != null)
    {
      user.UpdateBio(request.Bio);
    }

    // Update image if provided (can be null to clear)
    if (request.Image != null)
    {
      user.UpdateImage(request.Image);
    }

    // Save changes using UserManager
    var updateResult = await _userManager.UpdateAsync(user);
    if (!updateResult.Succeeded)
    {
      _logger.LogError("Error during user update for {UserId}: {Errors}",
        request.UserId,
        string.Join(", ", updateResult.Errors.Select(e => e.Description)));
      return Result.Error("An error occurred during update");
    }

    _logger.LogInformation("User {Username} updated successfully", user.Username);

    return Result.Success(user);
  }
}
