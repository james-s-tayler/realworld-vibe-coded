using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;

namespace Server.Web.Identity.Register;

/// <summary>
/// Register endpoint that creates a new user account
/// </summary>
public class Register(
  UserManager<ApplicationUser> userManager,
  SignInManager<ApplicationUser> signInManager) : Endpoint<RegisterRequest>
{
  public override void Configure()
  {
    Post("/api/identity/register");
    AllowAnonymous();
  }

  public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
  {
    // Validate email format
    if (string.IsNullOrEmpty(req.Email) || !IsValidEmail(req.Email))
    {
      var errors = new List<string> { "Invalid email address." };
      ThrowError(string.Join(" ", errors), StatusCodes.Status400BadRequest);
      return;
    }

    // Create user with email as username (matching Identity API behavior)
    var user = new ApplicationUser
    {
      UserName = req.Email,
      Email = req.Email,
    };

    var result = await userManager.CreateAsync(user, req.Password);

    if (!result.Succeeded)
    {
      var errors = result.Errors.Select(e => e.Description).ToList();
      ThrowError(string.Join(" ", errors), StatusCodes.Status400BadRequest);
      return;
    }

    // Sign in the user immediately (matching Identity API behavior)
    await signInManager.SignInAsync(user, isPersistent: false);

    // Return empty 200 OK response (matching Identity API behavior)
    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
  }

  private static bool IsValidEmail(string email)
  {
    try
    {
      var addr = new System.Net.Mail.MailAddress(email);
      return addr.Address == email;
    }
    catch
    {
      return false;
    }
  }
}
