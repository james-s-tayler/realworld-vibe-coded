using Finbuckle.MultiTenant;

namespace FinbuckleMultitenantAuditNet;

/// <summary>
/// POC entity for testing tenant-scoped data.
/// </summary>
[MultiTenant]
public class PocArticle
{
    public Guid Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Body { get; set; } = string.Empty;
    
    public string? TenantId { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
