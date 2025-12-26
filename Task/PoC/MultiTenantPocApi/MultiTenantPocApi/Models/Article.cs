namespace MultiTenantPocApi.Models;

/// <summary>
/// Article entity for POC - tenant-scoped data
/// Inherits from EntityBase which provides Id, CreatedAt, UpdatedAt, and TenantId
/// </summary>
public class Article : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
