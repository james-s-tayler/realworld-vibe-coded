using Microsoft.Extensions.Logging;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.UseCases.Interfaces;

namespace Server.UseCases.Users.Update;

public class UpdateUserHandler : ICommandHandler<UpdateUserCommand, User>
{
  private readonly IRepository<User> _repository;
  private readonly IPasswordHasher _passwordHasher;
  private readonly ILogger<UpdateUserHandler> _logger;

  public UpdateUserHandler(
    IRepository<User> repository,
    IPasswordHasher passwordHasher,
    ILogger<UpdateUserHandler> logger)
  {
    _repository = repository;
    _passwordHasher = passwordHasher;
    _logger = logger;
  }

  public async Task<Result<User>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Updating user {UserId}", request.UserId);

    var user = await _repository.GetByIdAsync(request.UserId, cancellationToken);

    if (user == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result<User>.NotFound();
    }

    // Check for duplicate email
    if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
    {
      var existingUserByEmail = await _repository
        .FirstOrDefaultAsync(new UserByEmailSpec(request.Email), cancellationToken);

      if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
      {
        _logger.LogWarning("Update failed: Email {Email} already exists", request.Email);
        return Result<User>.Invalid(new ErrorDetail
        {
          Identifier = "email",
          ErrorMessage = "Email already exists",
        });
      }

      user.UpdateEmail(request.Email);
    }

    // Check for duplicate username
    if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
    {
      var existingUserByUsername = await _repository
        .FirstOrDefaultAsync(new UserByUsernameSpec(request.Username), cancellationToken);

      if (existingUserByUsername != null && existingUserByUsername.Id != user.Id)
      {
        _logger.LogWarning("Update failed: Username {Username} already exists", request.Username);
        return Result<User>.Invalid(new ErrorDetail
        {
          Identifier = "username",
          ErrorMessage = "Username already exists",
        });
      }

      user.UpdateUsername(request.Username);
    }

    // Update password if provided
    if (!string.IsNullOrEmpty(request.Password))
    {
      var hashedPassword = _passwordHasher.HashPassword(request.Password);
      user.UpdatePassword(hashedPassword);
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

    await _repository.UpdateAsync(user, cancellationToken);

    _logger.LogInformation("User {Username} updated successfully", user.Username);

    return Result<User>.Success(user);
  }
}
