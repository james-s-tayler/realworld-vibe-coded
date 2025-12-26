using MultiTenantPocApi.Endpoints;

namespace MultiTenantPocApi.FunctionalTests;

/// <summary>
/// Tests verifying cross-tenant isolation - the core multi-tenancy requirement
/// These tests prove that data created in one tenant is NOT visible in another tenant
/// Each test uses unique tenant IDs to ensure proper isolation without database resets
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
        // Arrange - Use unique tenant IDs for this test
        var tenant1Id = $"test-tenant-1a-{Guid.NewGuid():N}";
        var tenant2Id = $"test-tenant-2a-{Guid.NewGuid():N}";
        
        var tenant1Client = _fixture.CreateTenantClient(tenant1Id);
        var tenant2Client = _fixture.CreateTenantClient(tenant2Id);

        var createRequest = new CreateArticleRequest
        {
            Title = "Tenant 1 Article",
            Body = "This article belongs to tenant-1"
        };

        // Act - Create article in tenant-1
        var createResponse = await tenant1Client.PostAsJsonAsync("/api/articles", createRequest);
        createResponse.EnsureSuccessStatusCode();
        
        var createdArticle = await createResponse.Content.ReadFromJsonAsync<ArticleResponse>();
        createdArticle.ShouldNotBeNull();
        createdArticle.TenantId.ShouldBe(tenant1Id);

        // Act - Query articles in tenant-2
        var listResponse = await tenant2Client.GetAsync("/api/articles");
        listResponse.EnsureSuccessStatusCode();
        
        var articlesInTenant2 = await listResponse.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Assert - Tenant-2 should NOT see tenant-1's article
        articlesInTenant2.ShouldNotBeNull();
        articlesInTenant2.ArticlesCount.ShouldBe(0);
        articlesInTenant2.Articles.ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateArticle_InTenant2_NotVisibleInTenant1()
    {
        // Arrange - Use unique tenant IDs for this test
        var tenant1Id = $"test-tenant-1b-{Guid.NewGuid():N}";
        var tenant2Id = $"test-tenant-2b-{Guid.NewGuid():N}";
        
        var tenant1Client = _fixture.CreateTenantClient(tenant1Id);
        var tenant2Client = _fixture.CreateTenantClient(tenant2Id);

        var createRequest = new CreateArticleRequest
        {
            Title = "Tenant 2 Article",
            Body = "This article belongs to tenant-2"
        };

        // Act - Create article in tenant-2
        var createResponse = await tenant2Client.PostAsJsonAsync("/api/articles", createRequest);
        createResponse.EnsureSuccessStatusCode();
        
        var createdArticle = await createResponse.Content.ReadFromJsonAsync<ArticleResponse>();
        createdArticle.ShouldNotBeNull();
        createdArticle.TenantId.ShouldBe(tenant2Id);

        // Act - Query articles in tenant-1
        var listResponse = await tenant1Client.GetAsync("/api/articles");
        listResponse.EnsureSuccessStatusCode();
        
        var articlesInTenant1 = await listResponse.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Assert - Tenant-1 should NOT see tenant-2's article
        articlesInTenant1.ShouldNotBeNull();
        articlesInTenant1.ArticlesCount.ShouldBe(0);
        articlesInTenant1.Articles.ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateArticles_InBothTenants_EachTenantSeesOnlyTheirOwn()
    {
        // Arrange - Use unique tenant IDs for this test
        var tenant1Id = $"test-tenant-1c-{Guid.NewGuid():N}";
        var tenant2Id = $"test-tenant-2c-{Guid.NewGuid():N}";
        
        var tenant1Client = _fixture.CreateTenantClient(tenant1Id);
        var tenant2Client = _fixture.CreateTenantClient(tenant2Id);

        // Act - Create 2 articles in tenant-1
        var tenant1Request1 = new CreateArticleRequest
        {
            Title = "Tenant 1 Article 1",
            Body = "First article in tenant-1"
        };
        var tenant1Request2 = new CreateArticleRequest
        {
            Title = "Tenant 1 Article 2",
            Body = "Second article in tenant-1"
        };

        await tenant1Client.PostAsJsonAsync("/api/articles", tenant1Request1);
        await tenant1Client.PostAsJsonAsync("/api/articles", tenant1Request2);

        // Act - Create 3 articles in tenant-2
        var tenant2Request1 = new CreateArticleRequest
        {
            Title = "Tenant 2 Article 1",
            Body = "First article in tenant-2"
        };
        var tenant2Request2 = new CreateArticleRequest
        {
            Title = "Tenant 2 Article 2",
            Body = "Second article in tenant-2"
        };
        var tenant2Request3 = new CreateArticleRequest
        {
            Title = "Tenant 2 Article 3",
            Body = "Third article in tenant-2"
        };

        await tenant2Client.PostAsJsonAsync("/api/articles", tenant2Request1);
        await tenant2Client.PostAsJsonAsync("/api/articles", tenant2Request2);
        await tenant2Client.PostAsJsonAsync("/api/articles", tenant2Request3);

        // Act - Query articles in each tenant
        var tenant1Response = await tenant1Client.GetAsync("/api/articles");
        var tenant2Response = await tenant2Client.GetAsync("/api/articles");

        var tenant1Articles = await tenant1Response.Content.ReadFromJsonAsync<ArticlesListResponse>();
        var tenant2Articles = await tenant2Response.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Assert - Tenant-1 sees only 2 articles
        tenant1Articles.ShouldNotBeNull();
        tenant1Articles.ArticlesCount.ShouldBe(2);
        tenant1Articles.Articles.ShouldAllBe(a => a.TenantId == tenant1Id);
        tenant1Articles.Articles.ShouldAllBe(a => a.Title.StartsWith("Tenant 1"));

        // Assert - Tenant-2 sees only 3 articles
        tenant2Articles.ShouldNotBeNull();
        tenant2Articles.ArticlesCount.ShouldBe(3);
        tenant2Articles.Articles.ShouldAllBe(a => a.TenantId == tenant2Id);
        tenant2Articles.Articles.ShouldAllBe(a => a.Title.StartsWith("Tenant 2"));
    }

    [Fact(Skip = "POC allows article creation without tenant context - production will require tenant")]
    public async Task CreateArticle_WithoutTenantHeader_CreatesWithEmptyTenant()
    {
        // This test is skipped in POC because we allow creation without tenant for simplicity
        // In production, this should either:
        // 1. Reject the request (return 400/401)
        // 2. Use a default/fallback tenant
        // 3. Get tenant from authenticated user claims
        
        // For POC validation, we only need to prove cross-tenant isolation works
        // See other tests for that validation
    }

    [Fact]
    public async Task QueryArticles_SameTenantMultipleTimes_ReturnsConsistentResults()
    {
        // Arrange - Use unique tenant ID for this test
        var tenantId = $"test-tenant-1d-{Guid.NewGuid():N}";
        
        var tenant1Client = _fixture.CreateTenantClient(tenantId);

        var createRequest = new CreateArticleRequest
        {
            Title = "Consistent Test Article",
            Body = "Testing consistency"
        };

        await tenant1Client.PostAsJsonAsync("/api/articles", createRequest);

        // Act - Query multiple times
        var response1 = await tenant1Client.GetAsync("/api/articles");
        var response2 = await tenant1Client.GetAsync("/api/articles");
        var response3 = await tenant1Client.GetAsync("/api/articles");

        var articles1 = await response1.Content.ReadFromJsonAsync<ArticlesListResponse>();
        var articles2 = await response2.Content.ReadFromJsonAsync<ArticlesListResponse>();
        var articles3 = await response3.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Assert - All queries return same results
        articles1.ShouldNotBeNull();
        articles2.ShouldNotBeNull();
        articles3.ShouldNotBeNull();

        articles1.ArticlesCount.ShouldBe(1);
        articles2.ArticlesCount.ShouldBe(1);
        articles3.ArticlesCount.ShouldBe(1);

        articles1.Articles[0].Id.ShouldBe(articles2.Articles[0].Id);
        articles2.Articles[0].Id.ShouldBe(articles3.Articles[0].Id);
        
        // Verify all articles have the correct tenant ID
        articles1.Articles[0].TenantId.ShouldBe(tenantId);
        articles2.Articles[0].TenantId.ShouldBe(tenantId);
        articles3.Articles[0].TenantId.ShouldBe(tenantId);
    }
}
