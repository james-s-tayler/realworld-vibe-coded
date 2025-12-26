namespace MultiTenantPocApi.Models;

/// <summary>
/// Article entity for POC - tenant-scoped data
/// In Finbuckle v10, no [MultiTenant] attribute needed - use .IsMultiTenant() in OnModelCreating
/// </summary>
public class Article
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? TenantId { get; set; } // Finbuckle manages this automatically
    public DateTime CreatedAt { get; set; }
}
