using FastEndpoints.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantPocApi.Data;

namespace MultiTenantPocApi.FunctionalTests;

/// <summary>
/// Test fixture for POC API with multi-tenant support
/// Each test uses unique tenant IDs for proper isolation - no database resets needed
/// </summary>
public class PocApiFixture : AppFixture<Program>
{
    /// <summary>
    /// Creates an HttpClient with X-Tenant-Id header set
    /// </summary>
    public HttpClient CreateTenantClient(string tenantId)
    {
        return CreateClient(client =>
        {
            client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        });
    }
}
