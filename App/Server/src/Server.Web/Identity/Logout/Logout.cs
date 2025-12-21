using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;

namespace Server.Web.Identity.Logout;

/// <summary>
/// Logout endpoint that clears authentication cookies
/// </summary>
public class Logout(SignInManager<ApplicationUser> signInManager) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Post("/api/identity/logout");
    AllowAnonymous(); // Allow both authenticated and anonymous to call this
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
  {
    await signInManager.SignOutAsync();
    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
  }
}
