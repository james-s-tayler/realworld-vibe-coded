using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.Core.UserAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Update;

// PV014: This handler uses ASP.NET Identity's UserManager instead of the repository pattern.
// UserManager.UpdateAsync and ResetPasswordAsync perform database mutations internally. We also
// update the legacy User table for backward compatibility during the migration period.
#pragma warning disable PV014
public class UpdateUserHandler : ICommandHandler<UpdateUserCommand, User>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly Server.SharedKernel.Persistence.IRepository<User> _userRepository;
  private readonly ILogger<UpdateUserHandler> _logger;

  public UpdateUserHandler(
    UserManager<ApplicationUser> userManager,
    Server.SharedKernel.Persistence.IRepository<User> userRepository,
    ILogger<UpdateUserHandler> logger)
  {
    _userManager = userManager;
    _userRepository = userRepository;
    _logger = logger;
  }

  public async Task<Result<User>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Updating user {UserId}", request.UserId);

    var user = await _userManager.FindByIdAsync(request.UserId.ToString());

    if (user == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result<User>.NotFound();
    }

    // Check for duplicate email
    if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
    {
      var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);

      if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
      {
        _logger.LogWarning("Update failed: Email {Email} already exists", request.Email);
        return Result<User>.Invalid(new ErrorDetail
        {
          Identifier = "email",
          ErrorMessage = "Email already exists",
        });
      }

      user.Email = request.Email;
    }

    // Check for duplicate username
    if (!string.IsNullOrEmpty(request.Username) && request.Username != user.UserName)
    {
      var existingUserByUsername = await _userManager.FindByNameAsync(request.Username);

      if (existingUserByUsername != null && existingUserByUsername.Id != user.Id)
      {
        _logger.LogWarning("Update failed: Username {Username} already exists", request.Username);
        return Result<User>.Invalid(new ErrorDetail
        {
          Identifier = "username",
          ErrorMessage = "Username already exists",
        });
      }

      user.UserName = request.Username;
    }

    // Update password if provided
    if (!string.IsNullOrEmpty(request.Password))
    {
      var token = await _userManager.GeneratePasswordResetTokenAsync(user);
      var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.Password);

      if (!passwordResult.Succeeded)
      {
        _logger.LogWarning("Password update failed for user {UserId}: {Errors}", request.UserId, string.Join(", ", passwordResult.Errors.Select(e => e.Description)));
        return Result<User>.Invalid(new ErrorDetail
        {
          Identifier = "password",
          ErrorMessage = string.Join(", ", passwordResult.Errors.Select(e => e.Description)),
        });
      }
    }

    // Update bio if provided
    if (request.Bio != null)
    {
      user.Bio = request.Bio;
    }

    // Update image if provided (can be null to clear)
    if (request.Image != null)
    {
      user.Image = request.Image;
    }

    var result = await _userManager.UpdateAsync(user);

    if (!result.Succeeded)
    {
      _logger.LogWarning("User update failed for {UserId}: {Errors}", request.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
      return Result<User>.Invalid(new ErrorDetail
      {
        Identifier = "body",
        ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description)),
      });
    }

    _logger.LogInformation("User {Username} updated successfully", user.UserName);

    // Also update legacy User table for backward compatibility during migration
    var legacyUser = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
    if (legacyUser != null)
    {
      // Update legacy user with same changes
      if (!string.IsNullOrEmpty(request.Email))
      {
        legacyUser.UpdateEmail(request.Email);
      }

      if (!string.IsNullOrEmpty(request.Username))
      {
        legacyUser.UpdateUsername(request.Username);
      }

      if (request.Bio != null)
      {
        legacyUser.UpdateBio(request.Bio);
      }

      if (request.Image != null)
      {
        legacyUser.UpdateImage(request.Image);
      }

      // Don't update password in legacy table since it's managed by Identity
      await _userRepository.UpdateAsync(legacyUser, cancellationToken);
      _logger.LogInformation("Legacy User record updated for backward compatibility");
    }

    // Map ApplicationUser to legacy User for response
    var responseUser = MapToLegacyUser(user);
    return Result<User>.Success(responseUser);
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
