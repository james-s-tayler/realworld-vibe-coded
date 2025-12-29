using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Update;

// PV014: This handler uses ASP.NET Identity's UserManager to normalize email/username
// but saves via EF Core directly to avoid internal validation that triggers tenant filter queries.
#pragma warning disable PV014
public class UpdateUserHandler : ICommandHandler<UpdateUserCommand, ApplicationUser>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<UpdateUserHandler> _logger;
  private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
  private readonly AppDbContext _dbContext;

  public UpdateUserHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<UpdateUserHandler> logger,
    IPasswordHasher<ApplicationUser> passwordHasher,
    AppDbContext dbContext)
  {
    _userManager = userManager;
    _logger = logger;
    _passwordHasher = passwordHasher;
    _dbContext = dbContext;
  }

  public async Task<Result<ApplicationUser>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Updating user {UserId}", request.UserId);

    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

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
      user.SecurityStamp = Guid.NewGuid().ToString();
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

    // Save changes via EF Core directly to avoid UserManager validation
    await _dbContext.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("User {Username} updated successfully", user.UserName);

    return Result<ApplicationUser>.Success(user);
  }
}
#pragma warning restore PV014
