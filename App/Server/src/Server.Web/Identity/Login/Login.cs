using Server.Infrastructure;
using Server.UseCases.Identity.Login;

namespace Server.Web.Identity.Login;

public class Login(IMediator mediator) : Endpoint<LoginRequest>
{
  private const string UseCookiesQueryParam = "useCookies";
  private const string UseSessionCookiesQueryParam = "useSessionCookies";

  public override void Configure()
  {
    Post("/api/identity/login");
    AllowAnonymous();
  }

  public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
  {
    var useCookies = HttpContext.Request.Query[UseCookiesQueryParam].ToString().Equals("true", StringComparison.OrdinalIgnoreCase);
    var useSessionCookies = HttpContext.Request.Query[UseSessionCookiesQueryParam].ToString().Equals("true", StringComparison.OrdinalIgnoreCase);

    var command = new LoginCommand(req.Email, req.Password, useCookies, useSessionCookies);
    var result = await mediator.Send(command, ct);

    if (result.Status == ResultStatus.NoContent)
    {
      await Send.ResultValueAsync(result, ct);
    }
    else
    {
      await Send.ResultMapperAsync(
        result,
        loginResult => new LoginResponse
        {
          AccessToken = loginResult.AccessToken,
          ExpiresIn = loginResult.ExpiresIn,
          RefreshToken = loginResult.RefreshToken,
        },
        ct);
    }
  }
}
