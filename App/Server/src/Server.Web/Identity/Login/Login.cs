using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure;
using Server.Infrastructure.Data;
using Server.UseCases.Identity.Login;

namespace Server.Web.Identity.Login;

public class Login(
  IMediator mediator,
  IMultiTenantStore<TenantInfo> tenantStore,
  AppDbContext dbContext) : Endpoint<LoginRequest>
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
    var normalizedEmail = req.Email.ToUpperInvariant();
    var user = await dbContext.Users
      .IgnoreQueryFilters()
      .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, ct);

    if (user == null)
    {
      await HttpContext.Response.SendUnauthorizedAsync(ct);
      return;
    }

    var tenant = await tenantStore.GetByIdAsync(user.TenantId);
    if (tenant == null)
    {
      await HttpContext.Response.SendUnauthorizedAsync(ct);
      return;
    }

    HttpContext.SetTenantInfo(tenant, resetServiceProviderScope: true);

    var useCookies = HttpContext.Request.Query[UseCookiesQueryParam].ToString().Equals("true", StringComparison.OrdinalIgnoreCase);
    var useSessionCookies = HttpContext.Request.Query[UseSessionCookiesQueryParam].ToString().Equals("true", StringComparison.OrdinalIgnoreCase);

    var command = new LoginCommand(req.Email, req.Password, useCookies, useSessionCookies);
    var newMediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
    var result = await newMediator.Send(command, ct);

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
