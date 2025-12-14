using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.Core.UserAggregate;
using Server.SharedKernel.Persistence;
using Server.UseCases.Interfaces;

namespace Server.Infrastructure.Services;

/// <summary>
/// Synchronizes User and ApplicationUser entities during the Identity migration period.
/// This ensures both authentication systems (JWT and Identity) can work with the same logical user.
/// </summary>
public class UserIdentitySyncService
{
  private readonly IRepository<User> _userRepository;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IPasswordHasher _passwordHasher;
  private readonly ILogger<UserIdentitySyncService> _logger;

  public UserIdentitySyncService(
    IRepository<User> userRepository,
    UserManager<ApplicationUser> userManager,
    IPasswordHasher passwordHasher,
    ILogger<UserIdentitySyncService> logger)
  {
    _userRepository = userRepository;
    _userManager = userManager;
    _passwordHasher = passwordHasher;
    _logger = logger;
  }

  /// <summary>
  /// Creates a User entity from an ApplicationUser (Identity → Old system sync)
  /// </summary>
  public async Task<User> SyncApplicationUserToUser(ApplicationUser applicationUser, CancellationToken cancellationToken = default)
  {
    _logger.LogInformation(
      "Syncing ApplicationUser {ApplicationUserId} to User table",
      applicationUser.Id);

    // Check if User already exists
    var existingUser = await _userRepository.GetByIdAsync(applicationUser.Id, cancellationToken);
    if (existingUser != null)
    {
      _logger.LogInformation("User already exists with ID {UserId}", applicationUser.Id);
      return existingUser;
    }

    // Get password hash from ApplicationUser
    // Note: We can't decrypt the hash, so we'll use a placeholder that won't work
    // The old JWT authentication won't work for users created via Identity
    var passwordHash = applicationUser.PasswordHash ?? throw new InvalidOperationException("ApplicationUser must have a password hash");

    // Create User entity with ApplicationUser's data
    // Use email as username for users created via Identity (Identity doesn't have separate username)
    var username = applicationUser.UserName ?? applicationUser.Email ?? throw new InvalidOperationException("ApplicationUser must have email");
    var email = applicationUser.Email ?? throw new InvalidOperationException("ApplicationUser must have email");

    var user = new User(email, username, passwordHash);

    // Sync custom properties
    user.UpdateBio(applicationUser.Bio);
    user.UpdateImage(applicationUser.Image);

    var createdUser = await _userRepository.AddAsync(user, cancellationToken);

    _logger.LogInformation(
      "Created User {UserId} from ApplicationUser {ApplicationUserId}",
      createdUser.Id,
      applicationUser.Id);

    return createdUser;
  }

  /// <summary>
  /// Creates an ApplicationUser entity from a User (Old system → Identity sync)
  /// </summary>
  public async Task<ApplicationUser> SyncUserToApplicationUser(User user, string password, CancellationToken cancellationToken = default)
  {
    _logger.LogInformation(
      "Syncing User {UserId} to ApplicationUser table",
      user.Id);

    // Check if ApplicationUser already exists
    var existingApplicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
    if (existingApplicationUser != null)
    {
      _logger.LogInformation("ApplicationUser already exists with ID {UserId}", user.Id);
      return existingApplicationUser;
    }

    // Create ApplicationUser with User's data
    var applicationUser = new ApplicationUser
    {
      Id = user.Id,
      UserName = user.Email, // Identity uses email as username
      Email = user.Email,
      EmailConfirmed = true, // Auto-confirm for migrated users
      Bio = user.Bio,
      Image = user.Image,
    };

    // Create the ApplicationUser with password
    var result = await _userManager.CreateAsync(applicationUser, password);

    if (!result.Succeeded)
    {
      var errors = string.Join(", ", result.Errors.Select(e => e.Description));
      _logger.LogError(
        "Failed to create ApplicationUser from User {UserId}: {Errors}",
        user.Id,
        errors);
      throw new InvalidOperationException($"Failed to create ApplicationUser: {errors}");
    }

    _logger.LogInformation(
      "Created ApplicationUser {ApplicationUserId} from User {UserId}",
      applicationUser.Id,
      user.Id);

    return applicationUser;
  }

  /// <summary>
  /// Syncs ApplicationUser properties to User (for updates)
  /// </summary>
  public async Task SyncApplicationUserUpdatesToUser(Guid userId, CancellationToken cancellationToken = default)
  {
    var applicationUser = await _userManager.FindByIdAsync(userId.ToString());
    if (applicationUser == null)
    {
      _logger.LogWarning("ApplicationUser not found for sync: {UserId}", userId);
      return;
    }

    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
    if (user == null)
    {
      _logger.LogWarning("User not found for sync: {UserId}", userId);
      return;
    }

    // Sync properties
    if (applicationUser.Email != null && user.Email != applicationUser.Email)
    {
      user.UpdateEmail(applicationUser.Email);
    }

    if (applicationUser.UserName != null && user.Username != applicationUser.UserName)
    {
      user.UpdateUsername(applicationUser.UserName);
    }

    user.UpdateBio(applicationUser.Bio);
    user.UpdateImage(applicationUser.Image);

    await _userRepository.UpdateAsync(user, cancellationToken);

    _logger.LogInformation("Synced ApplicationUser updates to User {UserId}", userId);
  }
}
