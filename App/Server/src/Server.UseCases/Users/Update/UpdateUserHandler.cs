using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Update;

// PV014: This handler uses ASP.NET Identity's UserManager instead of the repository pattern.
// We use PasswordHasher directly and UpdateSecurityStampAsync to avoid internal validation
// that triggers tenant filter queries.
#pragma warning disable PV014
public class UpdateUserHandler : ICommandHandler<UpdateUserCommand, ApplicationUser>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<UpdateUserHandler> _logger;
  private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

  public UpdateUserHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<UpdateUserHandler> logger,
    IPasswordHasher<ApplicationUser> passwordHasher)
  {
    _userManager = userManager;
    _logger = logger;
    _passwordHasher = passwordHasher;
  }

  public async Task<Result<ApplicationUser>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Updating user {UserId}", request.UserId);

    var user = await _userManager.FindByIdAsync(request.UserId.ToString());

    if (user == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result<ApplicationUser>.NotFound();
    }

    // Update email if provided (duplicate check done at endpoint level)
    if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
    {
      user.Email = request.Email;
      user.NormalizedEmail = _userManager.NormalizeEmail(request.Email);
    }

    // Update username if provided (duplicate check done at endpoint level)
    if (!string.IsNullOrEmpty(request.Username) && request.Username != user.UserName)
    {
      user.UserName = request.Username;
      user.NormalizedUserName = _userManager.NormalizeName(request.Username);
    }

    // Update password if provided
    if (!string.IsNullOrEmpty(request.Password))
    {
      user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
      await _userManager.UpdateSecurityStampAsync(user);
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

    // Use UpdateSecurityStampAsync to save changes without triggering validation
    // This updates the security stamp and saves the entity
    await _userManager.UpdateSecurityStampAsync(user);

    _logger.LogInformation("User {Username} updated successfully", user.UserName);

    return Result<ApplicationUser>.Success(user);
  }
}
#pragma warning restore PV014
