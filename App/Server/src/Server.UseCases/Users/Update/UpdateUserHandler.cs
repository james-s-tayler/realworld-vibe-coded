using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Users.Update;

// PV014: This handler uses ASP.NET Identity's UserManager instead of the repository pattern.
// UserManager.UpdateAsync and ResetPasswordAsync perform database mutations internally.
#pragma warning disable PV014
public class UpdateUserHandler : ICommandHandler<UpdateUserCommand, ApplicationUser>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IRepository<Author> _authorRepository;
  private readonly ILogger<UpdateUserHandler> _logger;

  public UpdateUserHandler(
    UserManager<ApplicationUser> userManager,
    IRepository<Author> authorRepository,
    ILogger<UpdateUserHandler> logger)
  {
    _userManager = userManager;
    _authorRepository = authorRepository;
    _logger = logger;
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

    // Check for duplicate email
    if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
    {
      var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);

      if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
      {
        _logger.LogWarning("Update failed: Email {Email} already exists", request.Email);
        return Result<ApplicationUser>.Invalid(new ErrorDetail
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
        return Result<ApplicationUser>.Invalid(new ErrorDetail
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
        return Result<ApplicationUser>.Invalid(new ErrorDetail
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
      return Result<ApplicationUser>.Invalid(new ErrorDetail
      {
        Identifier = "body",
        ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description)),
      });
    }

    // Sync Author record if any profile fields changed
    if (request.Username != null || request.Bio != null || request.Image != null)
    {
      var author = await _authorRepository.FirstOrDefaultAsync(
        new AuthorByUserIdSpec(user.Id), cancellationToken);

      if (author != null)
      {
        author.Update(user.UserName!, user.Bio, user.Image);
        await _authorRepository.UpdateAsync(author, cancellationToken);
        _logger.LogInformation("Author record updated for user {Username}", user.UserName);
      }
      else
      {
        _logger.LogWarning("No Author record found for user {UserId} during profile update", user.Id);
      }
    }

    _logger.LogInformation("User {Username} updated successfully", user.UserName);

    return Result<ApplicationUser>.Success(user);
  }
}
#pragma warning restore PV014
