using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using MultiTenantPocApi.Models;

namespace MultiTenantPocApi.Services;

/// <summary>
/// Adds TenantId claim to authenticated users for Finbuckle ClaimStrategy resolution
/// </summary>
public class TenantClaimsTransformation : IClaimsTransformation
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TenantClaimsTransformation> _logger;

    public TenantClaimsTransformation(
        UserManager<ApplicationUser> userManager,
        ILogger<TenantClaimsTransformation> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Check if TenantId claim already exists
        if (principal.HasClaim(c => c.Type == "TenantId"))
        {
            return principal;
        }

        // Get user from principal
        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
        {
            return principal;
        }

        // Get TenantId from user (Finbuckle adds this property via MultiTenantIdentityDbContext)
        // Use reflection to get TenantId since it's added by Finbuckle dynamically
        var tenantIdProperty = user.GetType().GetProperty("TenantId");
        if (tenantIdProperty == null)
        {
            _logger.LogWarning("TenantId property not found on user {UserId}", user.Id);
            return principal;
        }

        var tenantId = tenantIdProperty.GetValue(user) as string;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            _logger.LogWarning("User {UserId} has null or empty TenantId", user.Id);
            return principal;
        }

        // Add TenantId claim
        var identity = principal.Identity as ClaimsIdentity;
        if (identity != null)
        {
            identity.AddClaim(new Claim("TenantId", tenantId));
            _logger.LogInformation("Added TenantId claim '{TenantId}' for user {UserId}", tenantId, user.Id);
        }

        return principal;
    }
}
