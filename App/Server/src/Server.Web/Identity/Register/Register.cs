using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using FluentValidation.Results;
using Server.Infrastructure;
using Server.UseCases.Identity.Register;

namespace Server.Web.Identity.Register;

public class Register(IMediator mediator, IMultiTenantStore<TenantInfo> tenantStore) : Endpoint<RegisterRequest>
{
  public override void Configure()
  {
    Post("/api/identity/register");
    AllowAnonymous();
  }

  public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
  {
    // Create tenant BEFORE calling MediatR so scoped services see the tenant context
    var tenantId = Guid.NewGuid().ToString();
    var tenantIdentifier = tenantId;
    var tenant = new TenantInfo(tenantId, tenantIdentifier, req.Email);

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

    // Set tenant context using Finbuckle's HttpContext extension
    // This must happen BEFORE resolving any tenant-scoped services
    HttpContext.SetTenantInfo(tenant, resetServiceProviderScope: true);

    var command = new RegisterCommand(req.Email, req.Password, tenantId);
    var result = await mediator.Send(command, ct);

    await Send.ResultValueAsync(result, ct);
  }
}
