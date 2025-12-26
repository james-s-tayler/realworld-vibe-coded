using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace MultiTenantPocApi.Services;

/// <summary>
/// Adds TenantId claim to authenticated users for Finbuckle ClaimStrategy resolution
/// For POC: Uses in-memory storage to map user email to tenantId
/// In production: Store TenantId in database (user claims, separate tenant membership table, etc.)
/// </summary>
public class TenantClaimsTransformation : IClaimsTransformation
{
    // POC: In-memory storage of user email -> tenantId mappings
    // In production: Query this from database
    private static readonly ConcurrentDictionary<string, string> _userTenantMap = new();
    
    private readonly ILogger<TenantClaimsTransformation> _logger;

    public TenantClaimsTransformation(ILogger<TenantClaimsTransformation> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Register a user's tenant association (called from Register endpoint)
    /// </summary>
    public static void RegisterUserTenant(string email, string tenantId)
    {
        _userTenantMap[email] = tenantId;
    }

    /// <summary>
    /// Get a user's tenant association (called from Login endpoint)
    /// </summary>
    public static string GetUserTenant(string email)
    {
        return _userTenantMap.TryGetValue(email, out var tenantId) ? tenantId : string.Empty;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Check if TenantId claim already exists
        if (principal.HasClaim(c => c.Type == "TenantId"))
        {
            return Task.FromResult(principal);
        }

        // Get user email from claims
        var emailClaim = principal.FindFirst(ClaimTypes.Name) ?? principal.FindFirst(ClaimTypes.Email);
        if (emailClaim == null)
        {
            return Task.FromResult(principal);
        }

        // Look up tenant ID from in-memory map
        if (!_userTenantMap.TryGetValue(emailClaim.Value, out var tenantId))
        {
            _logger.LogWarning("No tenant mapping found for user {Email}", emailClaim.Value);
            return Task.FromResult(principal);
        }

        // Add TenantId claim
        var identity = principal.Identity as ClaimsIdentity;
        if (identity != null)
        {
            identity.AddClaim(new Claim("TenantId", tenantId));
            _logger.LogInformation("Added TenantId claim '{TenantId}' for user {Email}", tenantId, emailClaim.Value);
        }

        return Task.FromResult(principal);
    }
}
