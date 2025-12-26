using Microsoft.AspNetCore.Identity;

namespace MultiTenantPocApi.Models;

/// <summary>
/// Application user for POC - mimics the real app's ApplicationUser
/// Must inherit from IdentityUser for Identity integration
/// </summary>
public class ApplicationUser : IdentityUser
{
    // Tenants will be isolated via Finbuckle's automatic filtering
}
