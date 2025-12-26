using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using MultiTenantPocApi.Models;

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
            await SendAsync(new LoginResponse(), 401, ct);
            return;
        }

        var result = await SignInManager.PasswordSignInAsync(user, req.Password, isPersistent: false, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            await SendAsync(new LoginResponse(), 401, ct);
            return;
        }

        // Get TenantId using reflection
        var tenantIdProperty = user.GetType().GetProperty("TenantId");
        var tenantId = tenantIdProperty?.GetValue(user) as string ?? string.Empty;

        await SendOkAsync(new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            TenantId = tenantId
        }, ct);
    }
}
