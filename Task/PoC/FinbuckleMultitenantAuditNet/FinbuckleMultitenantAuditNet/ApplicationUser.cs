using Microsoft.AspNetCore.Identity;

namespace FinbuckleMultitenantAuditNet;

/// <summary>
/// POC ApplicationUser for testing multi-tenancy with Identity.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string? TenantId { get; set; }
}
