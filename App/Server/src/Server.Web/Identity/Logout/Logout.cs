using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;

namespace Server.Web.Identity.Logout;

/// <summary>
/// Logout endpoint for cookie-based authentication
/// </summary>
/// <remarks>
/// Signs out the currently authenticated user by clearing the authentication cookie.
/// This endpoint is needed for MapIdentityApi cookie authentication since the default
/// MapIdentityApi doesn't provide a logout endpoint for cookie-based auth.
/// </remarks>
public class Logout(SignInManager<ApplicationUser> signInManager) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Post("/api/identity/logout");
    AllowAnonymous(); // Allow anonymous so unauthenticated calls don't fail
    Summary(s =>
    {
      s.Summary = "Logout current user";
      s.Description = "Signs out the currently authenticated user by clearing the authentication cookie.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    // SignOut will clear the authentication cookie
    await signInManager.SignOutAsync();

    // Return 200 OK with no content
    HttpContext.Response.StatusCode = 200;
  }
}
