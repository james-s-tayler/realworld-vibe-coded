using Microsoft.Extensions.Logging;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Users.Register;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, User>
{
  private readonly IRepository<User> _repository;
  private readonly IPasswordHasher _passwordHasher;
  private readonly ILogger<RegisterUserHandler> _logger;

  public RegisterUserHandler(
    IRepository<User> repository,
    IPasswordHasher passwordHasher,
    ILogger<RegisterUserHandler> logger)
  {
    _repository = repository;
    _passwordHasher = passwordHasher;
    _logger = logger;
  }

  public async Task<Result<User>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling user registration for {Email}", request.Email);

    // Check if user already exists by email or username
    var existingUserByEmail = await _repository
      .FirstOrDefaultAsync(new UserByEmailSpec(request.Email), cancellationToken);

    if (existingUserByEmail != null)
    {
      _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
      return Result<User>.Invalid(new ErrorDetail
      {
        Identifier = nameof(request.Email),
        ErrorMessage = "Email already exists",
      });
    }

    var existingUserByUsername = await _repository
      .FirstOrDefaultAsync(new UserByUsernameSpec(request.Username), cancellationToken);

    if (existingUserByUsername != null)
    {
      _logger.LogWarning("Registration failed: Username {Username} already exists", request.Username);
      return Result<User>.Invalid(new ErrorDetail
      {
        Identifier = nameof(request.Username),
        ErrorMessage = "Username already exists",
      });
    }

    // Hash password and create user
    var hashedPassword = _passwordHasher.HashPassword(request.Password);
    var newUser = new User(request.Email, request.Username, hashedPassword);

    try
    {
      var createdUser = await _repository.AddAsync(newUser, cancellationToken);

      _logger.LogInformation("User {Username} registered successfully with ID {UserId}",
        createdUser.Username, createdUser.Id);

      return Result<User>.Created(createdUser);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during user registration for {Email}", request.Email);
      return Result<User>.Error(new ErrorDetail("Error", "An error occurred during registration"));
    }
  }
}
