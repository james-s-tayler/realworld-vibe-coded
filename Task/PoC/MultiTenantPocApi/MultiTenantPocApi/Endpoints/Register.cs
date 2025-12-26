using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using MultiTenantPocApi.Models;
using MultiTenantPocApi.Services;

namespace MultiTenantPocApi.Endpoints;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty; // User specifies which tenant they belong to
}

public class RegisterResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}

public class Register : Endpoint<RegisterRequest, RegisterResponse>
{
    public UserManager<ApplicationUser> UserManager { get; set; } = null!;
    public SignInManager<ApplicationUser> SignInManager { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        // Create user
        var user = new ApplicationUser
        {
            UserName = req.Email,
            Email = req.Email
        };

        // For POC: Don't set TenantId on user entity - Identity entities are not multi-tenant
        // Instead, register the tenant association in TenantClaimsTransformation
        TenantClaimsTransformation.RegisterUserTenant(req.Email, req.TenantId);

        var result = await UserManager.CreateAsync(user, req.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            HttpContext.Response.StatusCode = 400;
            await Send.StringAsync(errors, cancellation: ct);
            return;
        }

        // Sign in the user (adds authentication cookie/claims)
        await SignInManager.SignInAsync(user, isPersistent: false);

        await Send.OkAsync(new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            TenantId = req.TenantId
        }, ct);
    }
}
