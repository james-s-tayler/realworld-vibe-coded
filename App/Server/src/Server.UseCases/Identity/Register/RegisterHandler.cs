using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Identity.Register;

#pragma warning disable SRV015
public class RegisterHandler : ICommandHandler<RegisterCommand, Unit>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<RegisterHandler> _logger;

  public RegisterHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<RegisterHandler> logger)
  {
    _userManager = userManager;
    _logger = logger;
  }

#pragma warning disable PV014

  public async Task<Result<Unit>> Handle(RegisterCommand request, CancellationToken cancellationToken)

#pragma warning restore PV014

  {
    _logger.LogInformation("Creating user with email {Email}", request.Email);

    var user = new ApplicationUser
    {
      UserName = request.Email,
      Email = request.Email,
    };

    var result = await _userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
      var errorDetails = result.Errors.Select(e => new ErrorDetail("email", e.Description)).ToArray();
      _logger.LogWarning("User registration failed for {Email}: {Errors}", request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
      return Result<Unit>.Invalid(errorDetails);
    }

    _logger.LogInformation("User {Email} created successfully", request.Email);
    return Result<Unit>.Success(Unit.Value);
  }
}
