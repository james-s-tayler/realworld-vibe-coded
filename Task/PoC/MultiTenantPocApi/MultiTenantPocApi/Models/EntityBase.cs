namespace MultiTenantPocApi.Models;

/// <summary>
/// Base entity class for POC - provides common properties for all entities
/// Similar to the real app's EntityBase but simplified for POC purposes
/// </summary>
public abstract class EntityBase
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? TenantId { get; set; } // Finbuckle manages this automatically
}
