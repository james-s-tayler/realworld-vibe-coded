using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.Core.UserAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.GetCurrent;

public class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, User>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<GetCurrentUserHandler> _logger;

  public GetCurrentUserHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<GetCurrentUserHandler> logger)
  {
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<Result<User>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Getting current user for ID {UserId}", request.UserId);

    // Look up ApplicationUser by ID
    var appUser = await _userManager.FindByIdAsync(request.UserId.ToString());

    if (appUser == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result<User>.NotFound();
    }

    _logger.LogInformation("Retrieved current user {Username}", appUser.UserName);

    // Map ApplicationUser to legacy User for backward compatibility
    var legacyUser = MapToLegacyUser(appUser);
    return Result<User>.Success(legacyUser);
  }

  private static User MapToLegacyUser(ApplicationUser appUser)
  {
    // Create a User entity with a dummy hashed password since we're using Identity now
    // This is for backward compatibility with code that expects a User entity
    var user = new User(appUser.Email!, appUser.UserName!, "identity-managed");

    // Set the ID to match the ApplicationUser ID
    typeof(User).GetProperty("Id")!.SetValue(user, appUser.Id);

    user.UpdateBio(appUser.Bio);
    user.UpdateImage(appUser.Image);

    return user;
  }
}
