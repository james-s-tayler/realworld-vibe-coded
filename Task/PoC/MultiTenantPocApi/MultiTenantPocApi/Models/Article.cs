using Finbuckle.MultiTenant;

namespace MultiTenantPocApi.Models;

/// <summary>
/// Article entity for POC - tenant-scoped data
/// </summary>
[MultiTenant]
public class Article
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? TenantId { get; set; } // Finbuckle manages this automatically
    public DateTime CreatedAt { get; set; }
}
