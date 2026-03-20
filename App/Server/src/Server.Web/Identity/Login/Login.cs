using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Server.Infrastructure;
using Server.UseCases.Identity.Login;

namespace Server.Web.Identity.Login;

public class Login(IMediator mediator) : Endpoint<LoginRequest>
{
  public override void Configure()
  {
    Post("/api/identity/login");
    AllowAnonymous();
  }

  public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
  {
    var command = new LoginCommand
    {
      Email = req.Email,
      Password = req.Password,
      UseCookies = req.UseCookies == true,
      UseSessionCookies = req.UseSessionCookies == true,
    };
    var result = await mediator.Send(command, ct);

    if (!result.IsSuccess)
    {
      await Send.ResultValueAsync(result, ct);
      return;
    }

    var loginResult = result.Value;

    if (loginResult.RequiresCookieAuth && loginResult.Principal != null)
    {
      var authProperties = new AuthenticationProperties
      {
        IsPersistent = loginResult.IsPersistent,
      };

      await HttpContext.SignInAsync(
        IdentityConstants.ApplicationScheme,
        loginResult.Principal,
        authProperties);

      await HttpContext.Response.SendOkAsync(cancellation: ct);
    }
    else
    {
      await HttpContext.Response.SendOkAsync(
        new LoginResponse
        {
          AccessToken = loginResult.AccessToken,
          ExpiresIn = loginResult.ExpiresIn,
          RefreshToken = loginResult.RefreshToken,
        },
        cancellation: ct);
    }
  }
}
