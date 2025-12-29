using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.Infrastructure;
using Server.Infrastructure.Data;
using Server.UseCases.Identity.Register;

namespace Server.Web.Identity.Register;

public class Register(IMultiTenantStore<TenantInfo> tenantStore) : Endpoint<RegisterRequest>
{
  public override void Configure()
  {
    Post("/api/identity/register");
    AllowAnonymous();
  }

  public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
  {
    var tenantId = Guid.NewGuid().ToString();
    var tenant = new TenantInfo(tenantId, tenantId, req.Email);

    var added = await tenantStore.AddAsync(tenant);
    if (!added)
    {
      await HttpContext.Response.SendErrorsAsync(
        new List<ValidationFailure>
        {
          new ValidationFailure("tenant", "Failed to create tenant"),
        },
        statusCode: 422,
        cancellation: ct);
      return;
    }

    HttpContext.SetTenantInfo(tenant, resetServiceProviderScope: true);

    var mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
    var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var dbContext = HttpContext.RequestServices.GetRequiredService<AppDbContext>();

    const string ownerRoleName = "Owner";
    if (!await roleManager.RoleExistsAsync(ownerRoleName))
    {
      var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(ownerRoleName));
      if (!roleResult.Succeeded)
      {
        await HttpContext.Response.SendErrorsAsync(
          new List<ValidationFailure>
          {
            new ValidationFailure("role", "Failed to create Owner role"),
          },
          statusCode: 500,
          cancellation: ct);
        return;
      }
    }

    var command = new RegisterCommand(req.Email, req.Password);
    var result = await mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      var user = await userManager.FindByEmailAsync(req.Email);
      if (user != null)
      {
        var entry = dbContext.Entry(user);
        entry.Property("TenantId").CurrentValue = tenantId;
        await dbContext.SaveChangesAsync(ct);

        await userManager.AddToRoleAsync(user, ownerRoleName);
      }
    }

    await Send.ResultValueAsync(result, ct);
  }
}
