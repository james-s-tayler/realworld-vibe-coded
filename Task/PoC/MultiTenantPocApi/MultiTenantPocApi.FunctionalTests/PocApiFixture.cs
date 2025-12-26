using System.Net.Http.Json;
using FastEndpoints.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantPocApi.Data;
using MultiTenantPocApi.Endpoints;

namespace MultiTenantPocApi.FunctionalTests;

/// <summary>
/// Test fixture for POC API with multi-tenant support using ClaimStrategy
/// Tests use registration + login to get authenticated HttpClient with tenant context
/// </summary>
public class PocApiFixture : AppFixture<Program>
{
    /// <summary>
    /// Registers a new user and returns authenticated HttpClient with cookie
    /// </summary>
    public async Task<HttpClient> RegisterAndLoginUserAsync(string tenantId, string email, string password)
    {
        var client = CreateClient();
        
        // Register user with TenantId
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = password,
            TenantId = tenantId
        };

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        // Client now has authentication cookie from registration (SignInManager.SignInAsync)
        // The cookie contains user identity, and IClaimsTransformation will add TenantId claim
        return client;
    }
}
