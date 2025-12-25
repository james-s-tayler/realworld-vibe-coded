using Microsoft.AspNetCore.Identity;

namespace MultiTenantPocApi.Models;

/// <summary>
/// Application user for POC - mimics the real app's ApplicationUser
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    // Tenants will be isolated via Finbuckle's automatic filtering
}
