using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.SharedKernel.Resources;
using Server.UseCases.Interfaces;
using Server.UseCases.Users.Dtos;

namespace Server.UseCases.Users.Update;

// PV014: This handler uses ASP.NET Identity's UserManager instead of the repository pattern.
// UserManager.UpdateAsync and ResetPasswordAsync perform database mutations internally.
#pragma warning disable PV014
public class UpdateUserHandler : ICommandHandler<UpdateUserCommand, UserWithRolesDto>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IRepository<Author> _authorRepository;
  private readonly IQueryApplicationUsers _queryApplicationUsers;
  private readonly ILogger<UpdateUserHandler> _logger;
  private readonly IStringLocalizer _localizer;

  public UpdateUserHandler(
    UserManager<ApplicationUser> userManager,
    IRepository<Author> authorRepository,
    IQueryApplicationUsers queryApplicationUsers,
    ILogger<UpdateUserHandler> logger,
    IStringLocalizer localizer)
  {
    _userManager = userManager;
    _authorRepository = authorRepository;
    _queryApplicationUsers = queryApplicationUsers;
    _logger = logger;
    _localizer = localizer;
  }

  public async Task<Result<UserWithRolesDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Updating user {UserId}", request.UserId);

    var user = await _userManager.FindByIdAsync(request.UserId.ToString());

    if (user == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result<UserWithRolesDto>.NotFound();
    }

    // Check for duplicate email
    if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
    {
      var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);

      if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
      {
        _logger.LogWarning("Update failed: Email {Email} already exists", request.Email);
        return Result<UserWithRolesDto>.Invalid(new ErrorDetail
        {
          Identifier = "email",
          ErrorMessage = _localizer[SharedResource.Keys.EmailAlreadyExists],
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
        return Result<UserWithRolesDto>.Invalid(new ErrorDetail
        {
          Identifier = "username",
          ErrorMessage = _localizer[SharedResource.Keys.UsernameAlreadyExists],
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
        return Result<UserWithRolesDto>.Invalid(new ErrorDetail
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

    if (request.Language != null)
    {
      user.Language = request.Language;
    }

    var result = await _userManager.UpdateAsync(user);

    if (!result.Succeeded)
    {
      _logger.LogWarning("User update failed for {UserId}: {Errors}", request.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
      return Result<UserWithRolesDto>.Invalid(new ErrorDetail
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

      if (author == null)
      {
        _logger.LogError("Author record not found for user {UserId} - data integrity violation", user.Id);
        return Result<UserWithRolesDto>.Error(new ErrorDetail("author", _localizer[SharedResource.Keys.AuthorNotFound]));
      }

      author.Update(user.UserName!, user.Bio ?? string.Empty, user.Image);
      await _authorRepository.UpdateAsync(author, cancellationToken);
      _logger.LogInformation("Author record synced for user {Username}", user.UserName);
    }

    _logger.LogInformation("User {Username} updated successfully", user.UserName);

    var userWithRoles = await _queryApplicationUsers.GetCurrentUserWithRoles(request.UserId, cancellationToken);

    return Result<UserWithRolesDto>.Success(userWithRoles!);
  }
}
#pragma warning restore PV014
