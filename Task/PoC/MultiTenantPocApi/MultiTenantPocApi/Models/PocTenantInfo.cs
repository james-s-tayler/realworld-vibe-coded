namespace MultiTenantPocApi.Models;

/// <summary>
/// Tenant information for POC  
/// In v10, TenantInfo changed from a class to a record
/// </summary>
public record PocTenantInfo
{
    public required string Id { get; set; }
    public required string Identifier { get; set; }
    public required string Name { get; set; }
}
