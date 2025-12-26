using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using MultiTenantPocApi.Models;
using MultiTenantPocApi.Services;

namespace MultiTenantPocApi.Endpoints;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}

public class Login : Endpoint<LoginRequest, LoginResponse>
{
    public UserManager<ApplicationUser> UserManager { get; set; } = null!;
    public SignInManager<ApplicationUser> SignInManager { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await UserManager.FindByEmailAsync(req.Email);
        if (user == null)
        {
            HttpContext.Response.StatusCode = 401;
            await Send.StringAsync("Invalid email or password", cancellation: ct);
            return;
        }

        var result = await SignInManager.PasswordSignInAsync(user, req.Password, isPersistent: false, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            HttpContext.Response.StatusCode = 401;
            await Send.StringAsync("Invalid email or password", cancellation: ct);
            return;
        }

        // Get TenantId from in-memory map (POC approach)
        // In production: Query from database user claims or tenant membership table
        var tenantId = TenantClaimsTransformation.GetUserTenant(user.Email!);

        await Send.OkAsync(new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            TenantId = tenantId
        }, ct);
    }
}
