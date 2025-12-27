using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Identity.Register;

public class RegisterHandler : ICommandHandler<RegisterCommand, ApplicationUser>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly ILogger<RegisterHandler> _logger;

  public RegisterHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<RegisterHandler> logger)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _logger = logger;
  }

  // PV014: UserManager.CreateAsync is a mutation operation, but the analyzer doesn't recognize it
  // as a repository method. Suppressing this false positive.
#pragma warning disable PV014
  public async Task<Result<ApplicationUser>> Handle(RegisterCommand request, CancellationToken cancellationToken)
#pragma warning restore PV014
  {
    _logger.LogInformation("Registering new user with email {Email}", request.Email);

    var user = new ApplicationUser
    {
      UserName = request.Email,
      Email = request.Email,
    };

    var result = await _userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
      var errorDetails = result.Errors.Select(e => new ErrorDetail(e.Description)).ToArray();
      _logger.LogWarning("User registration failed for {Email}: {Errors}", request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
      return Result<ApplicationUser>.Invalid(errorDetails);
    }

    await _signInManager.SignInAsync(user, isPersistent: false);

    _logger.LogInformation("User {Email} registered and signed in successfully", request.Email);

    return Result<ApplicationUser>.Success(user);
  }
}
