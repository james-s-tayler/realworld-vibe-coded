using System.Net.Http.Json;
using MultiTenantPocApi.Endpoints;

namespace MultiTenantPocApi.FunctionalTests;

/// <summary>
/// Tests verifying cross-tenant isolation using ClaimStrategy authentication
/// These tests prove that data created in one tenant is NOT visible in another tenant
/// Tests use registration + login to establish authenticated context with TenantId claim
/// </summary>
[Collection("POC Sequential Tests")]
public class CrossTenantIsolationTests : IClassFixture<PocApiFixture>
{
    private readonly PocApiFixture _fixture;

    public CrossTenantIsolationTests(PocApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateArticle_InTenant1_NotVisibleInTenant2()
    {
        // Arrange - Register users in two different tenants
        var tenant1User = await _fixture.RegisterAndLoginUserAsync(
            "tenant-1", 
            $"user1a-{Guid.NewGuid():N}@test.com", 
            "Password123!");
        
        var tenant2User = await _fixture.RegisterAndLoginUserAsync(
            "tenant-2", 
            $"user2a-{Guid.NewGuid():N}@test.com", 
            "Password123!");

        var createRequest = new CreateArticleRequest
        {
            Title = "Tenant 1 Article",
            Body = "This article belongs to tenant-1"
        };

        // Act - Create article as tenant-1 user
        var createResponse = await tenant1User.PostAsJsonAsync("/api/articles", createRequest);
        createResponse.EnsureSuccessStatusCode();
        
        var createdArticle = await createResponse.Content.ReadFromJsonAsync<ArticleResponse>();
        createdArticle.ShouldNotBeNull();
        createdArticle.TenantId.ShouldBe("tenant-1");

        // Act - Query articles as tenant-2 user
        var listResponse = await tenant2User.GetAsync("/api/articles");
        listResponse.EnsureSuccessStatusCode();
        
        var articlesInTenant2 = await listResponse.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Assert - Tenant-2 should NOT see tenant-1's article
        articlesInTenant2.ShouldNotBeNull();
        articlesInTenant2.Articles.ShouldNotContain(a => a.Id == createdArticle.Id);
    }

    [Fact]
    public async Task CreateArticle_InTenant2_NotVisibleInTenant1()
    {
        // Arrange - Register users in two different tenants
        var tenant1User = await _fixture.RegisterAndLoginUserAsync(
            "tenant-1", 
            $"user1b-{Guid.NewGuid():N}@test.com", 
            "Password123!");
        
        var tenant2User = await _fixture.RegisterAndLoginUserAsync(
            "tenant-2", 
            $"user2b-{Guid.NewGuid():N}@test.com", 
            "Password123!");

        var createRequest = new CreateArticleRequest
        {
            Title = "Tenant 2 Article",
            Body = "This article belongs to tenant-2"
        };

        // Act - Create article as tenant-2 user
        var createResponse = await tenant2User.PostAsJsonAsync("/api/articles", createRequest);
        createResponse.EnsureSuccessStatusCode();
        
        var createdArticle = await createResponse.Content.ReadFromJsonAsync<ArticleResponse>();
        createdArticle.ShouldNotBeNull();
        createdArticle.TenantId.ShouldBe("tenant-2");

        // Act - Query articles as tenant-1 user
        var listResponse = await tenant1User.GetAsync("/api/articles");
        listResponse.EnsureSuccessStatusCode();
        
        var articlesInTenant1 = await listResponse.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Assert - Tenant-1 should NOT see tenant-2's article
        articlesInTenant1.ShouldNotBeNull();
        articlesInTenant1.Articles.ShouldNotContain(a => a.Id == createdArticle.Id);
    }

    [Fact]
    public async Task CreateArticles_InBothTenants_EachTenantSeesOnlyTheirOwn()
    {
        // Arrange - Register users in two different tenants
        var tenant1User = await _fixture.RegisterAndLoginUserAsync(
            "tenant-1", 
            $"user1c-{Guid.NewGuid():N}@test.com", 
            "Password123!");
        
        var tenant2User = await _fixture.RegisterAndLoginUserAsync(
            "tenant-2", 
            $"user2c-{Guid.NewGuid():N}@test.com", 
            "Password123!");

        // Act - Create 2 articles in tenant-1
        var tenant1Article1 = await tenant1User.PostAsJsonAsync("/api/articles", new CreateArticleRequest
        {
            Title = "Tenant 1 Article 1",
            Body = "First article in tenant-1"
        });
        tenant1Article1.EnsureSuccessStatusCode();

        var tenant1Article2 = await tenant1User.PostAsJsonAsync("/api/articles", new CreateArticleRequest
        {
            Title = "Tenant 1 Article 2",
            Body = "Second article in tenant-1"
        });
        tenant1Article2.EnsureSuccessStatusCode();

        // Act - Create 2 articles in tenant-2
        var tenant2Article1 = await tenant2User.PostAsJsonAsync("/api/articles", new CreateArticleRequest
        {
            Title = "Tenant 2 Article 1",
            Body = "First article in tenant-2"
        });
        tenant2Article1.EnsureSuccessStatusCode();

        var tenant2Article2 = await tenant2User.PostAsJsonAsync("/api/articles", new CreateArticleRequest
        {
            Title = "Tenant 2 Article 2",
            Body = "Second article in tenant-2"
        });
        tenant2Article2.EnsureSuccessStatusCode();

        // Act - Query articles in tenant-1
        var tenant1List = await tenant1User.GetAsync("/api/articles");
        tenant1List.EnsureSuccessStatusCode();
        var tenant1Articles = await tenant1List.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Act - Query articles in tenant-2
        var tenant2List = await tenant2User.GetAsync("/api/articles");
        tenant2List.EnsureSuccessStatusCode();
        var tenant2Articles = await tenant2List.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Assert - Each tenant sees only their own articles
        tenant1Articles.ShouldNotBeNull();
        tenant1Articles.ArticlesCount.ShouldBeGreaterThanOrEqualTo(2); // At least the 2 we created
        tenant1Articles.Articles.ShouldAllBe(a => a.TenantId == "tenant-1");

        tenant2Articles.ShouldNotBeNull();
        tenant2Articles.ArticlesCount.ShouldBeGreaterThanOrEqualTo(2); // At least the 2 we created
        tenant2Articles.Articles.ShouldAllBe(a => a.TenantId == "tenant-2");
    }

    [Fact]
    public async Task QueryArticles_SameTenantMultipleTimes_ReturnsConsistentResults()
    {
        // Arrange - Register user in tenant-1
        var tenant1User = await _fixture.RegisterAndLoginUserAsync(
            "tenant-1", 
            $"user1d-{Guid.NewGuid():N}@test.com", 
            "Password123!");

        // Act - Create article
        var createResponse = await tenant1User.PostAsJsonAsync("/api/articles", new CreateArticleRequest
        {
            Title = "Consistency Test Article",
            Body = "Testing query consistency"
        });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ArticleResponse>();

        // Act - Query articles multiple times
        var query1 = await tenant1User.GetAsync("/api/articles");
        var query2 = await tenant1User.GetAsync("/api/articles");
        var query3 = await tenant1User.GetAsync("/api/articles");

        var articles1 = await query1.Content.ReadFromJsonAsync<ArticlesListResponse>();
        var articles2 = await query2.Content.ReadFromJsonAsync<ArticlesListResponse>();
        var articles3 = await query3.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Assert - All queries return consistent results
        articles1.ShouldNotBeNull();
        articles2.ShouldNotBeNull();
        articles3.ShouldNotBeNull();

        articles1.ArticlesCount.ShouldBe(articles2.ArticlesCount);
        articles2.ArticlesCount.ShouldBe(articles3.ArticlesCount);

        // All results should contain the created article
        articles1.Articles.ShouldContain(a => a.Id == created!.Id);
        articles2.Articles.ShouldContain(a => a.Id == created!.Id);
        articles3.Articles.ShouldContain(a => a.Id == created!.Id);
    }
}
