using Ardalis.Result;
using Ardalis.SharedKernel;
using Microsoft.Extensions.Logging;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;

namespace Server.UseCases.Users.Update;

public class UpdateUserHandler : ICommandHandler<UpdateUserCommand, Result<UserDto>>
{
  private readonly IRepository<User> _repository;
  private readonly IPasswordHasher _passwordHasher;
  private readonly IJwtTokenGenerator _jwtTokenGenerator;
  private readonly ILogger<UpdateUserHandler> _logger;

  public UpdateUserHandler(
    IRepository<User> repository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    ILogger<UpdateUserHandler> logger)
  {
    _repository = repository;
    _passwordHasher = passwordHasher;
    _jwtTokenGenerator = jwtTokenGenerator;
    _logger = logger;
  }

  public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Updating user {UserId}", request.UserId);

    var user = await _repository.GetByIdAsync(request.UserId, cancellationToken);

    if (user == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result.NotFound();
    }

    // Check for duplicate email
    if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
    {
      var existingUserByEmail = await _repository
        .FirstOrDefaultAsync(new UserByEmailSpec(request.Email), cancellationToken);
      
      if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
      {
        _logger.LogWarning("Update failed: Email {Email} already exists", request.Email);
        return Result.Invalid(new ValidationError
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
        return Result.Invalid(new ValidationError
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

    try
    {
      await _repository.UpdateAsync(user, cancellationToken);
      var token = _jwtTokenGenerator.GenerateToken(user);

      _logger.LogInformation("User {Username} updated successfully", user.Username);

      return Result.Success(new UserDto(
        user.Id,
        user.Email,
        user.Username,
        user.Bio,
        user.Image,
        token));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during user update for {UserId}", request.UserId);
      return Result.Error("An error occurred during update");
    }
  }
}