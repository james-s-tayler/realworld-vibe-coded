using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using MultiTenantPocApi.Models;

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

        // Set TenantId using reflection (Finbuckle adds this property dynamically)
        var tenantIdProperty = user.GetType().GetProperty("TenantId");
        if (tenantIdProperty != null)
        {
            tenantIdProperty.SetValue(user, req.TenantId);
        }

        var result = await UserManager.CreateAsync(user, req.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            await SendAsync(new RegisterResponse(), 400, ct);
            return;
        }

        // Sign in the user (adds authentication cookie/claims)
        await SignInManager.SignInAsync(user, isPersistent: false);

        await SendOkAsync(new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            TenantId = req.TenantId
        }, ct);
    }
}
